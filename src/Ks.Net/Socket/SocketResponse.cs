namespace Ks.Net.Socket;

sealed class SocketResponse
{
    public static SocketResponse Empty = new();
    public string Message { get; set; }
}
