using Ks.Net.Kestrel;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket.Middlewares;

/// <summary>
/// 回退处理中间件
/// </summary>
sealed class FallbackMiddleware(ILogger<FallbackMiddleware> logger) 
    : ISocketMiddleware
{
    public Task InvokeAsync(NetDelegate<SocketContext> next, SocketContext context)
    {
        logger.LogWarning(context.Request.Message?.ToString());
        if (context.Request.Message != null)
        {
            return context.Client.WriteAsync(context.Request.Message);
        }
        
        return Task.CompletedTask;
    }
}
