using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using Ks.Core.System.Buffers;
using Ks.Net.Kestrel;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket.Telnet;

public class TelnetClient(ILogger<TelnetClient> logger, NetDelegate<SocketContext> net)
    : ISocketClient
{
    internal ConnectionContext Context { get; set; }
    
    internal PipeWriter Writer => Context.Transport.Output;
    
    public bool IsClose()
    {
        return Context.ConnectionClosed.IsCancellationRequested;
    }

    public Task WriteAsync<T>(T message)
    {
        switch (message)
        {
            case string s:
                Writer.Write(s, Encoding.UTF8);
                break;
            default:
                Writer.Write(message?.ToString(), Encoding.UTF8);
                break;
        }
        
        Writer.WriteCRLF();
        return Writer.FlushAsync().AsTask();
    }

    public Task StartAsync()
    {
        logger.LogInformation($"[{Context.ConnectionId}]加入连接.");
        return HandleRequestsAsync(Context);
    }

    public Task StopAsync()
    {
        logger.LogInformation($"[{Context.ConnectionId}]断开连接.");
        return Context.DisposeAsync().AsTask();
    }
    
    private async Task HandleRequestsAsync(ConnectionContext context)
    {
        var input = context.Transport.Input;
        while (context.ConnectionClosed.IsCancellationRequested == false)
        {
            var result = await input.ReadAsync();
            if (result.IsCanceled)
            {
                break;
            }

            if (TryReadRequest(result, out var line, out var consumed))
            {
                input.AdvanceTo(consumed);

                var response = new SocketResponse();
                var request = new SocketRequest { Message = line };
                var socketConnect = new SocketContext(this, request, response, context.Features);
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
    
    private bool TryReadRequest(ReadResult result, out string request, out SequencePosition consumed)
    {
        var reader = new SequenceReader<byte>(result.Buffer);
        if (reader.TryReadTo(out ReadOnlySpan<byte> span, Constants.Crlf))
        {
            request = Encoding.UTF8.GetString(span);
            consumed = reader.Position;
            return true;
        }
        else
        {
            request = string.Empty;
            consumed = result.Buffer.Start;
            return false;
        }
    }
}