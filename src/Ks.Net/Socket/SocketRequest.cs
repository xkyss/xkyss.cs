namespace Ks.Net.Socket;

sealed class SocketRequest
{
    public static SocketRequest Empty = new SocketRequest();
    public string Message { get; set; }
}
