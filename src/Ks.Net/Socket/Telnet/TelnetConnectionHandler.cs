using Ks.Net.Socket.Server;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket.Telnet;

public class TelnetConnectionHandler(IServiceProvider sp, ILogger<TelnetConnectionHandler> logger)
    : ConnectionHandler
{
public override async Task OnConnectedAsync(ConnectionContext context)
{
    var client = sp.GetRequiredService<TelnetClient>();
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