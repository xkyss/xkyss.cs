using Ks.Net.Kestrel;
using Ks.Net.SocketServer.Middlewares;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;


namespace Ks.Net.SocketServer;

public class ServerConnectionHandler : ConnectionHandler 
{
    private readonly ILogger<ServerConnectionHandler> logger;
    private readonly NetDelegate<SocketServerContext> net;

    public ServerConnectionHandler(IServiceProvider sp, ILogger<ServerConnectionHandler> logger)
    {
        this.logger = logger;
        this.net = new NetBuilder<SocketServerContext>(sp)
            .Use<FallbackMiddlware>()
            .Build();
    }

    public override async Task OnConnectedAsync(ConnectionContext context)
    {
        logger.LogInformation("OnConnectedAsync");
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

            // Parse requests
            var requests = new List<SocketRequest>() { new SocketRequest(), new SocketRequest() }; // RedisRequest.Parse(result.Buffer, out var consumed);
            if (requests.Count > 0)
            {
                foreach (var request in requests)
                {
                    var response = new SocketResponse();
                    var redisContext = new SocketServerContext(client, request, response, context.Features);
                    await this.net.Invoke(redisContext);
                }
                //input.AdvanceTo(consumed);
            }
            else
            {
                //input.AdvanceTo(result.Buffer.Start, result.Buffer.End);
            }

            if (result.IsCompleted)
            {
                break;
            }
        }
    }
}