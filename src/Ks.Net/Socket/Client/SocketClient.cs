using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using Ks.Core;
using Ks.Net.Kestrel;
using Ks.Net.Socket.Client.Middlewares;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket.Client;

internal sealed class SocketClient(
    ILogger<SocketClient> logger
    , IConfiguration configuration
    , ISocketDecoder decoder
    , ISocketEncoder encoder
    , ISocketTypeMapper typeMapper
    , NetDelegate<SocketContext> net
)   : ISocketClient
{
    private readonly CancellationTokenSource CloseTokenSource = new();
    private readonly Pipe _receivePipe = new();
    private readonly TcpClient _socket = new (AddressFamily.InterNetwork)
    {
        NoDelay = true
    };
    
    internal Stream Writer => _socket.GetStream();
    
    public bool IsClose() => CloseTokenSource.IsCancellationRequested;
    
    public Task WriteAsync<T>(T message)
    {
        if (message == null)
        {
            logger.LogWarning("消息为空.");
            return Task.CompletedTask;
        }
        
        var type = message.GetType();
        if (!typeMapper.TryGet(type, out var id))
        {
            logger.LogWarning($"消息{type}未注册.");
            return Task.CompletedTask;
        }
        
        var bodyBytes = encoder.Encode(type, message);
        var headerBytes = encoder.Encode(new SocketRequest
        {
            MessageTypeId = id,
            MessageLength = bodyBytes.Length
        });
        
        // 写入响应头长度（4字节，大端序）
        var headerLengthBytes = BitConverter.GetBytes(headerBytes.Length);
        if (BitConverter.IsLittleEndian)
        {
            // 确保使用大端序
            Array.Reverse(headerLengthBytes);
        }

        if (headerLengthBytes.Length != 4)
        {
            throw new ArgumentException("headerLengthBytes.Length != 4");
        }
        Writer.Write(headerLengthBytes, 0, headerLengthBytes.Length);
        Writer.Write(headerBytes);
        Writer.Write(bodyBytes);
        return Writer.FlushAsync(CloseTokenSource.Token);
    }
    
    public async Task StartAsync()
    {
        var serverConfig = configuration.GetSection(Constants.DefaultSocketClientKey);
        var ip = serverConfig.GetValue(Constants.DefaultSocketHostKey, Constants.DefaultSocketHost)!;
        var port = serverConfig.GetValue(Constants.DefaultSocketPortKey, Constants.DefaultSocketPort);
        
        await _socket.ConnectAsync(ip, port);
        _ = ReceiveNetAsync();
        _ = ReceivePipAsync();
    }

    public Task StopAsync()
    {
        CloseTokenSource.Cancel();
        return Task.CompletedTask;
    }

    private async Task ReceivePipAsync()
    {
        var input = _receivePipe.Reader;
        var cancelToken = CloseTokenSource.Token;
        while (cancelToken.IsCancellationRequested == false)
        {
            var result = await input.ReadAsync(cancelToken);
            if (result.IsCanceled)
            {
                break;
            }

            if (TryReadResponse(result, out var response, out var consumed))
            {
                var request = new SocketRequest();
                var socketContext = new SocketContext(this, request, response, null);
                await net.Invoke(socketContext);
                input.AdvanceTo(consumed);
            }
            else
            {
                input.AdvanceTo(result.Buffer.Start, result.Buffer.End);
            }

            if (result.IsCompleted)
            {
                break;
            }
        }
    }
    
    private async Task ReceiveNetAsync()
    {
        try
        {
            var readBuffer = new byte[2048];
            var dataPipeWriter = _receivePipe.Writer;
            var cancelToken = CloseTokenSource.Token;
            var stream = _socket.GetStream();
            while (!cancelToken.IsCancellationRequested)
            {
                var hasData = false;
                // logger.LogInformation("Receive with net.");
                do
                {
                    var length = await stream.ReadAsync(readBuffer, cancelToken);
                    if (length <= 0)
                    {
                        break;
                    }
                    
                    hasData = true;
                    dataPipeWriter.Write(readBuffer.AsSpan()[..length]);

                } while (stream.DataAvailable);

                if (hasData)
                {
                    var flushTask = dataPipeWriter.FlushAsync(cancelToken);
                    if (!flushTask.IsCompleted)
                    {
                        await flushTask.ConfigureAwait(false);
                    }
                }
                else
                {
                    break;
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return;
        };
        
        logger.LogInformation("ReceiveOnceAsync 结束.");
    }

    private bool TryReadResponse(ReadResult result, out SocketResponse response, out SequencePosition consumed)
    {
        var reader = new SequenceReader<byte>(result.Buffer);
        
        response = SocketResponse.Empty;
        consumed = result.Buffer.Start;

        // 消息头部长度
        if (!reader.TryReadBigEndian(out int headLen))
        {
            return false;
        }
        
        // 检测长度
        if (headLen <= 0)
        {
            return false;
        }

        if (!reader.TryReadExact(headLen, out var headerBytes))
        {
            return false;
        }

        try
        {
            response = decoder.Decode<SocketResponse>(headerBytes);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "解析[SocketResponse]失败");
            return false;
        }
        
        // 检测长度
        if (response.MessageLength <= 0)
        {
            return false;
        }

        // 读取Message
        if (!reader.TryReadExact(response.MessageLength, out var bodyBytes))
        {
            return false;
        }

        if (!typeMapper.TryGet(response.MessageTypeId, out var type))
        {
            return false;
        }
        
        response.Message = decoder.Decode(type, bodyBytes);
        consumed = reader.Position;
        return true;
    }
}