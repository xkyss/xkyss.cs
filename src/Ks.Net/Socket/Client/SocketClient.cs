using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using Ks.Net.Kestrel;
using Ks.Net.Socket.Middlewares;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket;

public class SocketClient(IServiceProvider sp, ILogger<SocketClient> logger, IConfiguration configuration)
{
    private readonly NetDelegate<SocketClientContext> net = new NetBuilder<SocketClientContext>(sp)
        .Use(c => cc =>
        {
            Console.WriteLine($"<: {cc.Response.Message}");
            return Task.CompletedTask;
        })
        .Build();
    
    protected readonly CancellationTokenSource CloseTokenSource = new();
    private readonly Pipe _receivePipe = new();
    private readonly TcpClient _socket = new (AddressFamily.InterNetwork)
    {
        NoDelay = true
    };
    
    public bool IsClose() => CloseTokenSource.IsCancellationRequested;

    public void Close() => CloseTokenSource.Cancel();
    
    public Task WriteAsync(ReadOnlySpan<char> text, Encoding? encoding = null)
    {
        Write(text, encoding);
        return FlushAsync();
    }

    public SocketClient Write(ReadOnlySpan<char> text, Encoding? encoding = null)
    {
        _socket.GetStream().Write((encoding ?? Encoding.UTF8).GetBytes(text.ToArray()));
        return this;
    }
    
    public Task WriteLineAsync(ReadOnlySpan<char> text, Encoding? encoding = null)
    {
        WriteLine(text, encoding);
        return FlushAsync();
    }

    public SocketClient WriteLine(ReadOnlySpan<char> text, Encoding? encoding = null)
    {
        _socket.GetStream().Write((encoding ?? Encoding.UTF8).GetBytes(text.ToArray()));
        _socket.GetStream().Write(Encoding.UTF8.GetBytes("\r\n"));
        return this;
    }

    public Task FlushAsync()
    {
        return _socket.GetStream().FlushAsync(CloseTokenSource.Token);
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

            if (TryReadResponse(result, out var response, out var consumed))
            {
                var request = new SocketRequest();
                var socketConnect = new SocketClientContext(this, request, response);
                await net.Invoke(socketConnect);
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

    private static bool TryReadResponse(ReadResult result, out SocketResponse response, out SequencePosition consumed)
    {
        var reader = new SequenceReader<byte>(result.Buffer);
        if (reader.TryReadTo(out ReadOnlySpan<byte> span, Constants.CRLF))
        {
            response = new SocketResponse() { Message = Encoding.UTF8.GetString(span) };
            consumed = reader.Position;
            return true;
        }
        else
        {
            response = SocketResponse.Empty;
            consumed = result.Buffer.Start;
            return false;
        } 
    }
    
    
    protected virtual bool TryParseMessage(ref ReadOnlySequence<byte> input)
    {
        var s = Encoding.UTF8.GetString(input);
        if (s.IsNullOrEmpty())
        {
            return false;
        }
        
        // logger.LogInformation($"TryParseMessage: {s}");
        input = input.Slice(input.GetPosition(s.Length));
        return true;
    }
}