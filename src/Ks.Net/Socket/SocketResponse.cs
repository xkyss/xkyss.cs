namespace Ks.Net.Socket;

internal sealed class SocketResponse
{
    public static SocketResponse Empty = new();
    public string? Message { get; set; }
}
