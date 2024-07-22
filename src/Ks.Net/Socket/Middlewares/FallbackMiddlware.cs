using Ks.Net.Kestrel;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket.Middlewares;

/// <summary>
/// 回退处理中间件
/// </summary>
sealed class FallbackMiddlware(ILogger<FallbackMiddlware> logger) 
    : ISocketMiddleware
{
    public Task InvokeAsync(NetDelegate<SocketContext> next, SocketContext context)
    {
        logger.LogWarning(context.Request.Message);
        // context.Client.WriteLineAsync(context.Request.Message);
        return Task.CompletedTask;
    }
}
