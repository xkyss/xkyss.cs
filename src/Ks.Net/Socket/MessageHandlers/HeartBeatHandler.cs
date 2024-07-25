namespace Ks.Net.Socket.MessageHandlers;

public class HeartBeatHandler<TClient> : ISocketMessageHandler<TClient>
    where TClient : ISocketClient
{
    public Task HandleAsync(SocketContext<TClient> context)
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