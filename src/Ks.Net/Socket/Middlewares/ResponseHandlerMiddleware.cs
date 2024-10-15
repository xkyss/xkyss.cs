using Ks.Core;
using Ks.Net.Kestrel;

namespace Ks.Net.Socket.Middlewares;

public class ResponseHandlerMiddleware : ISocketMiddleware
{
    private readonly Dictionary<Type, ISocketMessageHandler> _handlers = new();

    public void Register<T>(ISocketMessageHandler handler)
    {
        Register(typeof(T), handler);
    }

    public void Register(Type type, ISocketMessageHandler handler)
    {
        _handlers[type] = handler;
    }

    public Task InvokeAsync(NetDelegate<SocketContext> next, SocketContext context)
    {
        Check.NotNull(context.Response.Message, "Message is null");
        
        if (_handlers.TryGetValue(context.Response.Message!.GetType(), out var hanler))
        {
            return hanler.HandleAsync(context);
        }
        else
        {
            return next(context);
        }
    }
}