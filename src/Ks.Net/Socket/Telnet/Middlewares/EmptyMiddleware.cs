using Ks.Net.Kestrel;

namespace Ks.Net.Socket.Telnet.Middlewares;

public class EmptyMiddleware : ISocketMiddleware
{
    public Task InvokeAsync(NetDelegate<SocketContext> next, SocketContext context)
    {
        var message = context.Request.Message;
        if (message == null || (message is string s && string.IsNullOrEmpty(s)))
        {
            return context.Client.WriteAsync("Please type something.");
        }
        else
        {
            return next(context);
        }
    }
}