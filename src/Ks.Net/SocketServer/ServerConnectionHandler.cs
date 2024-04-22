using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;


namespace Ks.Net.SocketServer;

public class ServerConnectionHandler : ConnectionHandler 
{
    private readonly ILogger<ServerConnectionHandler> logger;

    public override Task OnConnectedAsync(ConnectionContext connection)
    {
        logger.LogInformation("OnConnectedAsync");
        return Task.CompletedTask;
    }
}