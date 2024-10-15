namespace Ks.Net.Socket;

public interface ISocketMessageHandler
{
    Task HandleAsync(SocketContext context);
}