using Ks.Net.Socket.WebSocketServer;
using Microsoft.AspNetCore.Builder;

namespace Ks.Net.Socket.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MapWebSocket(this WebApplication app, string pattern)
    {
        app.MapConnectionHandler<WsServerConnectionHandler>(pattern);
        return app;
    }
}