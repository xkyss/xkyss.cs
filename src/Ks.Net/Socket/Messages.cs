using MessagePack;

namespace Ks.Net.Socket;


[MessagePackObject]
public class HeartBeat
{
    [Key(0)]
    public int Sid { get; set; }
    
    [Key(1)]
    public int TimeTick { get; set; }

    public override string ToString()
    {
        return $"[{GetType()}]: Sid: {Sid}, TimeTick: {TimeTick}";
    }
}