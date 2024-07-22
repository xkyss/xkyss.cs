namespace Ks.Net.Socket;

class SocketClientContext
{
    public SocketClient Client { get; }
    
    /// <summary>
    /// 请求
    /// </summary>
    public SocketRequest Request { get; }

    /// <summary>
    /// 响应
    /// </summary>
    public SocketResponse Response { get; }

    public SocketClientContext(SocketClient client, SocketRequest request, SocketResponse response) 
    {
        Client = client;
        Request = request;
        Response = response;
    }
}