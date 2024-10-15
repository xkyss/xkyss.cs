using MessagePack;

namespace Ks.Net.Socket;

[MessagePackObject]
public sealed class SocketRequest
{
    public static SocketRequest Empty = new();
    
    [Key(0)]
    public long TransportId { get; set; }
    
    [Key(1)]
    public int MessageTypeId { get; set; }
    
    [Key(2)]
    public int MessageLength { get; set; }
    
    [IgnoreMember]
    public object? Message { get; set; }
}
