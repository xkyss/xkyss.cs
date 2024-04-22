using Ks.Net.Kestrel;
using Microsoft.Extensions.Logging;

namespace Ks.Net.SocketServer.Middlewares;

/// <summary>
/// 回退处理中间件
/// </summary>
sealed class FallbackMiddlware(ILogger<FallbackMiddlware> logger) : ISocketServerMiddleware
{

    public Task InvokeAsync(NetDelegate<SocketServerContext> next, SocketServerContext context)
    {
        logger.LogWarning("Fallback");
        //this.logger.LogWarning($"无法处理{context.Reqeust}");
        //return context.Response.WriteAsync(ResponseContent.Err);
        return Task.CompletedTask;
    }
}
