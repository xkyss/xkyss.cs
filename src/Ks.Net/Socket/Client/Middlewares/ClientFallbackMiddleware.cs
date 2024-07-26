using Ks.Net.Kestrel;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket.Client.Middlewares;

/// <summary>
/// 回退处理中间件
/// </summary>
sealed class ClientFallbackMiddleware(ILogger<ClientFallbackMiddleware> logger) 
    : ISocketMiddleware
{
    public Task InvokeAsync(NetDelegate<SocketContext> next, SocketContext context)
    {
        logger.LogWarning($"<: {context.Response.Message}");
        return Task.CompletedTask;
    }
}
