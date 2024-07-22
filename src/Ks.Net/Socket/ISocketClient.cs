namespace Ks.Net.Socket;

public interface ISocketClient<out T> : ISocketWritable<T>
{
    bool IsClose();

    Task StartAsync();
    
    Task StopAsync();
}