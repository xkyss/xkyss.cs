using Ks.Net.Kestrel;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket.Middlewares;

/// <summary>
/// 回退处理中间件
/// </summary>
sealed class FallbackMiddleware<TClient>(ILogger<FallbackMiddleware<TClient>> logger) 
    : ISocketMiddleware<TClient>
    where TClient : ISocketClient<TClient>
{
    public Task InvokeAsync(NetDelegate<SocketContext<TClient>> next, SocketContext<TClient> context)
    {
        logger.LogWarning(context.Request.Message);
        context.Client.WriteLineAsync(context.Request.Message);
        return Task.CompletedTask;
    }
}
