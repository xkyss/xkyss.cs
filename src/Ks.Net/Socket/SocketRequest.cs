namespace Ks.Net.Socket;

internal sealed class SocketRequest
{
    public static SocketRequest Empty = new();
    public string? Message { get; set; }
}
