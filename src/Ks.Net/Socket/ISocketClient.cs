namespace Ks.Net.Socket;

public interface ISocketClient
{
    bool IsClose();

    void Write<T>(T message) where T : Message;

    Task StartAsync();
    
    Task StopAsync();
}