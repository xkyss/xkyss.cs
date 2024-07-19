using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using Ks.Net.Kestrel;
using Ks.Net.Socket.Middlewares;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket;

public class ServerConnectionHandler(IServiceProvider sp, ILogger<ServerConnectionHandler> logger)
    : ConnectionHandler
{
    private static readonly byte[] crlf = Encoding.ASCII.GetBytes("\r\n");

    private readonly NetDelegate<SocketServerContext> net = new NetBuilder<SocketServerContext>(sp)
        .Use<FallbackMiddlware>()
        .Build();

    public override async Task OnConnectedAsync(ConnectionContext context)
    {
        logger.LogInformation($"[{context.ConnectionId}]加入连接.");
        try
        {
            await HandleRequestsAsync(context);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex.Message);
        }
        finally
        {
            logger.LogInformation($"[{context.ConnectionId}]断开连接.");
            await context.DisposeAsync();
        }
    }

    private async Task HandleRequestsAsync(ConnectionContext context)
    {
        var input = context.Transport.Input;
        var client = new ConnectedClient(context);

        while (context.ConnectionClosed.IsCancellationRequested == false)
        {
            var result = await input.ReadAsync();
            if (result.IsCanceled)
            {
                break;
            }

            if (TryReadRequest(result, out var request, out var consumed))
            {
                var response = new SocketResponse();
                var socketConnect = new SocketServerContext(client, request, response, context.Features);
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