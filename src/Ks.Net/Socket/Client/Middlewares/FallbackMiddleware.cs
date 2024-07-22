using Ks.Net.Kestrel;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket.Client.Middlewares;

/// <summary>
/// 回退处理中间件
/// </summary>
sealed class FallbackMiddleware(ILogger<FallbackMiddleware> logger) 
    : ISocketMiddleware<SocketClient>
{
    public Task InvokeAsync(NetDelegate<SocketContext<SocketClient>> next, SocketContext<SocketClient> context)
    {
        logger.LogWarning($"<: {context.Response.Message}");
        return Task.CompletedTask;
    }
}
