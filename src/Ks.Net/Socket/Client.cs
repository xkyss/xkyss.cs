using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket;

public class Client(ILogger<Client> logger, IConfiguration configuration)
{
    protected readonly CancellationTokenSource CloseTokenSource = new();
    private readonly Pipe _receivePipe = new();
    private readonly TcpClient _socket = new (AddressFamily.InterNetwork)
    {
        NoDelay = true
    };
    
    public bool IsClose() => CloseTokenSource.IsCancellationRequested;

    public void Close() => CloseTokenSource.Cancel();

    public async Task WriteLine(string s)
    {
        await _socket.GetStream().WriteAsync(Encoding.UTF8.GetBytes(s));
        await _socket.GetStream().WriteAsync(Encoding.UTF8.GetBytes("\r\n"));
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
        try
        {
            var cancelToken = CloseTokenSource.Token;
            while (!CloseTokenSource.Token.IsCancellationRequested)
            {
                logger.LogInformation("Receive with pipe.");
                var result = await _receivePipe.Reader.ReadAsync(cancelToken);
                var buffer = result.Buffer;
                if (buffer.Length > 0)
                {
                    while (TryParseMessage(ref buffer))
                    {
                        
                    };
                    
                    _receivePipe.Reader.AdvanceTo(buffer.Start, buffer.End);
                }
                else if (result.IsCanceled || result.IsCompleted)
                {
                    break;
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return;
        }
        
        logger.LogInformation("ReceiveAsync 结束.");
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
                logger.LogInformation("Receive with net.");
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

    protected virtual bool TryParseMessage(ref ReadOnlySequence<byte> input)
    {
        var s = Encoding.UTF8.GetString(input);
        if (s.IsNullOrEmpty())
        {
            return false;
        }
        
        logger.LogInformation($"TryParseMessage: {s}");
        input = input.Slice(input.GetPosition(s.Length));
        return true;
    }
}