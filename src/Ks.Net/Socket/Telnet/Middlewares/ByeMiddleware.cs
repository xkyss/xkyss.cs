using Ks.Net.Kestrel;

namespace Ks.Net.Socket.Telnet.Middlewares;

public class ByeMiddleware : ISocketMiddleware
{
    public async Task InvokeAsync(NetDelegate<SocketContext> next, SocketContext context)
    {
        var message = context.Request.Message;
        if (message is string s && !string.IsNullOrEmpty(s) && s.Equals("bye", StringComparison.OrdinalIgnoreCase))
        {
            await context.Client.WriteAsync("Have a good day!");
            await context.Client.StopAsync();
        }
        else
        {
            await next(context);
        }
    }
}