namespace Ks.Net.Socket;

public interface ISocketClient
{
    bool IsClose();

    Task WriteAsync<T>(T message) where T : Message;

    Task StartAsync();
    
    Task StopAsync();
}