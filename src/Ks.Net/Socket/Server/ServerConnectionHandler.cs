﻿using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket.Server;

public class ServerConnectionHandler(IServiceProvider sp, ILogger<ServerConnectionHandler> logger)
    : ConnectionHandler
{
    public override async Task OnConnectedAsync(ConnectionContext context)
    {
        var client = sp.GetRequiredService<ServerClient>();
        client.Context = context;

        try
        {
            await client.StartAsync();
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex.Message);
        }
        finally
        {
            await client.StopAsync();
        }
    }
}