using Ks.Net.Socket.WebSocketServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;

namespace Ks.Net.Socket.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MapWebSocket(this WebApplication app, string pattern)
    {
        app.MapConnectionHandler<WsServerConnectionHandler>(pattern, options =>
        {
            options.Transports = HttpTransportType.WebSockets;
        });
        return app;
    }
}