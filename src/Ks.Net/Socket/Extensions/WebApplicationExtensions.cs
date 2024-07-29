using Ks.Net.Socket.Server;
using Microsoft.AspNetCore.Builder;

namespace Ks.Net.Socket.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MapSocketServer(this WebApplication app, string pattern)
    {
        app.MapConnectionHandler<ServerConnectionHandler>(pattern);
        return app;
    }
}