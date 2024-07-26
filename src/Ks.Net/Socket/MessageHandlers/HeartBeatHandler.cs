namespace Ks.Net.Socket.MessageHandlers;

public class HeartBeatHandler : ISocketMessageHandler
{
    public Task HandleAsync(SocketContext context)
    {
        if (context.Request.Message is HeartBeat hb)
        {
            return context.Client.WriteAsync(new HeartBeat()
            {
                Sid = hb.Sid,
                TimeTick = DateTime.Now.Microsecond,
            });
        }
        
        return Task.CompletedTask;
    }
}