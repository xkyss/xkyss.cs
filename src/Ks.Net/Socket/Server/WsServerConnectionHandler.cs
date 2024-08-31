using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket.Server;

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

            var transferFormatFeature = context.Features.Get<ITransferFormatFeature>();
            if (transferFormatFeature != null)
            {
                transferFormatFeature.ActiveFormat = TransferFormat.Binary;
            }

            var client = sp.GetRequiredService<ServerClient>();
            client.Context = context;

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