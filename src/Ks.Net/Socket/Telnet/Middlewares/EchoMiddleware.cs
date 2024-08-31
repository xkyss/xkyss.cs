using Ks.Net.Kestrel;

namespace Ks.Net.Socket.Telnet.Middlewares;

public class EchoMiddleware : ISocketMiddleware
{
    public Task InvokeAsync(NetDelegate<SocketContext> next, SocketContext context)
    {
        return context.Client.WriteAsync($"Did you say '{context.Request.Message}'?");
    }
}