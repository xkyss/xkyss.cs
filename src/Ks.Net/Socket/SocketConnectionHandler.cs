using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket
{
    public class SocketConnectionHandler(ILogger<SocketConnectionHandler> logger) : ConnectionHandler
    {
        public override Task OnConnectedAsync(ConnectionContext connection)
        {
            logger.LogDebug($"{connection.RemoteEndPoint} 链接成功. ConnectionId: ({connection.ConnectionId})");
            return Task.CompletedTask;
        }
    }
}