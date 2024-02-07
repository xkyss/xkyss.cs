using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ks.Net.Socket
{
    public class SocketConnectionHandler(ILogger<SocketConnectionHandler> logger) : ConnectionHandler
    {
        public override Task OnConnectedAsync(ConnectionContext connection)
        {
            logger.LogDebug($"{connection.RemoteEndPoint} 链接成功. ConnectionId: ({connection.ConnectionId})");
            var channel = new SocketServerChannel(connection, NullLogger<SocketServerChannel>.Instance);
            channel.OnMessageHandler += m =>
            {
                logger.LogInformation("OnMessage.");
                return Task.CompletedTask;
            };
            return channel.RunAsync();
        }
    }
}