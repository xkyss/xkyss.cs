using MessagePack;

namespace Ks.Net.Socket;

[MessagePackObject]
public class Message
{
    
    [Key(0)]
    public int Sid { get; set; }
}

[MessagePackObject]
public class HeartBeat : Message
{
    [Key(1)]
    public int TimeTick { get; set; }
}