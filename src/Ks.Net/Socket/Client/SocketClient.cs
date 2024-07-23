using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using Ks.Net.Kestrel;
using Ks.Net.Socket.Client.Middlewares;
using MessagePack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket.Client;

public class SocketClient(IServiceProvider sp, ILogger<SocketClient> logger, IConfiguration configuration)
    : ISocketClient
{
    private readonly NetDelegate<SocketContext<SocketClient>> net = new NetBuilder<SocketContext<SocketClient>>(sp)
        .Use<FallbackMiddleware>()
        .Build();
    
    protected readonly CancellationTokenSource CloseTokenSource = new();
    private readonly Pipe _receivePipe = new();
    private readonly TcpClient _socket = new (AddressFamily.InterNetwork)
    {
        NoDelay = true
    };
    
    internal Stream Writer => _socket.GetStream();
    
    public bool IsClose() => CloseTokenSource.IsCancellationRequested;
    
    public void Write<T>(T message) where T : Message
    {
        var bodyBytes = MessagePackSerializer.Serialize(message);
        
        var request = new SocketRequest();
        request.MessageLength = bodyBytes.Length;
        var headerBytes = MessagePackSerializer.Serialize(request);
        
        // 写入响应头长度（4字节，大端序）
        var headerLengthBytes = BitConverter.GetBytes(headerBytes.Length);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(headerLengthBytes); // 确保使用大端序
        }
        Writer.Write(headerLengthBytes, 0, headerLengthBytes.Length);
        Writer.Write(headerBytes);
        Writer.Write(bodyBytes);
        Writer.FlushAsync(CloseTokenSource.Token);
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
        while (CloseTokenSource.Token.IsCancellationRequested == false)
        {
            var result = await input.ReadAsync();
            if (result.IsCanceled)
            {
                break;
            }

            if (TryReadResponse(result, out var response))
            {
                var request = new SocketRequest();
                var socketConnect = new SocketContext<SocketClient>(this, request, response, null);
                await net.Invoke(socketConnect);
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

    private static bool TryReadResponse(ReadResult result, out SocketResponse response)
    {
        var reader = new SequenceReader<byte>(result.Buffer);
        
        response = SocketResponse.Empty;
        
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
        
        var headerBytes = result.Buffer.Slice(reader.Position, headLen);
        response = MessagePackSerializer.Deserialize<SocketResponse>(headerBytes);
        
        // 检测长度
        if (response.MessageLength <= 0)
        {
            return false;
        }

        // 读取Message
        // TODO: 当前写死HeartBeat
        reader.Advance(headLen);
        var bodyBytes = result.Buffer.Slice(reader.Position);
        response.Message = MessagePackSerializer.Deserialize<HeartBeat>(bodyBytes);
        return true;
    }
}