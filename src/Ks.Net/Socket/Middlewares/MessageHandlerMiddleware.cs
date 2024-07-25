using Ks.Core;
using Ks.Net.Kestrel;

namespace Ks.Net.Socket.Middlewares;

public class MessageHandlerMiddleware<TClient> : ISocketMiddleware<TClient>
    where TClient : ISocketClient
{
    private readonly Dictionary<Type, ISocketMessageHandler<TClient>> _handlers = new();

    public void Register<T>(ISocketMessageHandler<TClient> handler)
    {
        Register(typeof(T), handler);
    }

    public void Register(Type type, ISocketMessageHandler<TClient> handler)
    {
        _handlers[type] = handler;
    }

    public Task InvokeAsync(NetDelegate<SocketContext<TClient>> next, SocketContext<TClient> context)
    {
        Check.NotNull(context.Request.Message, "Message is null");
        
        if (_handlers.TryGetValue(context.Request.Message!.GetType(), out var hanler))
        {
            return hanler.HandleAsync(context);
        }
        else
        {
            return next(context);
        }
    }
}