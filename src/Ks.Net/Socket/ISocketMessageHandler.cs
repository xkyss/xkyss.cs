namespace Ks.Net.Socket;

public interface ISocketMessageHandler<TClient>
    where TClient : ISocketClient
{
    Task HandleAsync(SocketContext<TClient> context);
}