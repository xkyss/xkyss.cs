using System.Net.Sockets;

namespace Ks.Net.Socket;

public class SocketClient
{
    private readonly TcpClient _socket = new (AddressFamily.InterNetwork)
    {
        NoDelay = true
    };

    private NetChannel? _channel;

    public async Task Connect(string ip, int port)
    {
        try
        {
            await _socket.ConnectAsync(ip, port);
        }
        catch (Exception e)
        {
            // Log.Error(e);
            return;
        }

        _channel = new SocketClientChannel();
        await _channel.RunAsync();
    }
}