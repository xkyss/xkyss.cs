using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket.WebSocketServer;

public class WsServerConnectionHandler(IServiceProvider sp, ILogger<WsServerConnectionHandler> logger)
    : ConnectionHandler
{
    public override async Task OnConnectedAsync(ConnectionContext context)
    {
        if (context.GetHttpContext()?.WebSockets.IsWebSocketRequest == false)
        {
            logger.LogError("不是WebSocket连接");
            return;
        }
        var webSocket = await context.GetHttpContext()!.WebSockets.AcceptWebSocketAsync();

        var client = sp.GetRequiredService<WsServerClient>();
        client.Context = context;
        client.WebSocket = webSocket;

        try
        {
            await client.StartAsync();
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex.Message);
        }
        finally
        {
            await client.StopAsync();
        }
    }
}