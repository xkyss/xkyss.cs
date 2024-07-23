using MessagePack;

namespace Ks.Net.Socket;

[MessagePackObject]
public sealed class SocketResponse
{
    public static SocketResponse Empty = new();
    
    [Key(0)]
    public long TransportId { get; set; }
    
    [Key(1)]
    public long MessageTypeId { get; set; }
    
    [Key(2)]
    public int MessageLength { get; set; }
    
    [IgnoreMember]
    public Message? Message { get; set; }
}
