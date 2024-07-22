using Ks.Net.Kestrel;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket.Middlewares;

/// <summary>
/// 回退处理中间件
/// </summary>
sealed class FallbackMiddlware(ILogger<FallbackMiddlware> logger) 
    : ISocketServerMiddleware
{
    public Task InvokeAsync(NetDelegate<SocketServerContext> next, SocketServerContext context)
    {
        logger.LogWarning(context.Request.Message);
        context.Client.WriteLineAsync(context.Request.Message);
        return Task.CompletedTask;
    }
}
