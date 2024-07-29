using System.Net;
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
        try
        {
            if (context.GetHttpContext()?.WebSockets.IsWebSocketRequest == false)
            {
                logger.LogError("不是WebSocket连接");
                return;
            }
            var webSocket = await context.GetHttpContext()!.WebSockets.AcceptWebSocketAsync();
            
            var buffer = new ArraySegment<byte>(new byte[2048]);
            var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

            var client = sp.GetRequiredService<WsServerClient>();
            client.Context = context;
            client.WebSocket = webSocket;

            try
            {
                await client.StartAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
            finally
            {
                await client.StopAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }
        
    }
}