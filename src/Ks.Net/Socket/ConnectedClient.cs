using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using Ks.Net.Kestrel;
using Ks.Net.Socket.Middlewares;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket;

internal sealed class ConnectedClient(IServiceProvider sp, ILogger<ConnectedClient> logger)
{
    private static readonly byte[] crlf = Encoding.ASCII.GetBytes("\r\n");

    private readonly NetDelegate<SocketServerContext> net = new NetBuilder<SocketServerContext>(sp)
        .Use<FallbackMiddlware>()
        .Build();
    
    internal ConnectionContext Context { get; set; }

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

            if (TryReadRequest(result, out var request, out var consumed))
            {
                var response = new SocketResponse(context.Transport.Output);
                var socketConnect = new SocketServerContext(this, request, response, context.Features);
                await this.net.Invoke(socketConnect);
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

    private static bool TryReadRequest(ReadResult result, out SocketRequest request, out SequencePosition consumed)
    {
        var reader = new SequenceReader<byte>(result.Buffer);
        if (reader.TryReadTo(out ReadOnlySpan<byte> span, crlf))
        {
            request = new SocketRequest { Message = Encoding.UTF8.GetString(span) };
            consumed = reader.Position;
            return true;
        }
        else
        {
            request = SocketRequest.Empty;
            consumed = result.Buffer.Start;
            return false;
        }
    }
    
}
