using Ks.Net.Kestrel;
using Ks.Net.SocketServer.Middlewares;
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
        var socketServerContext = new SocketServerContext(context.Features);
        await net.Invoke(socketServerContext);
    }
}