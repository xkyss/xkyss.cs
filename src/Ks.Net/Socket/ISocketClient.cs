namespace Ks.Net.Socket;

public interface ISocketClient
{
    bool IsClose();

    Task WriteAsync<T>(T message);

    Task StartAsync();
    
    Task StopAsync();
}