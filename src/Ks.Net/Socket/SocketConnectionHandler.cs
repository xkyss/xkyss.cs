using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket
{
    public class SocketConnectionHandler(ILoggerFactory loggerFactory) : ConnectionHandler
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<SocketConnectionHandler>();
        public override Task OnConnectedAsync(ConnectionContext connection)
        {
            _logger.LogDebug($"{connection.RemoteEndPoint} 链接成功. ConnectionId: ({connection.ConnectionId})");
            var channel = new SocketServerChannel(connection, loggerFactory.CreateLogger<SocketServerChannel>());
            channel.OnMessageHandler += m =>
            {
                _logger.LogInformation("OnMessage.");
                channel.Write(new Message());
                return Task.CompletedTask;
            };
            return channel.RunAsync();
        }
    }
}