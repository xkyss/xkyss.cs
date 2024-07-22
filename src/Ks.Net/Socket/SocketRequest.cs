namespace Ks.Net.Socket;

sealed class SocketRequest
{
    public static SocketRequest Empty = new();
    public string Message { get; set; }
}
