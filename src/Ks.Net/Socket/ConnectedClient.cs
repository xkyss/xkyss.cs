using Microsoft.AspNetCore.Connections;

namespace Ks.Net.SocketServer;

sealed class ConnectedClient
{
    private readonly ConnectionContext context;

    public ConnectedClient(ConnectionContext context)
    {
        this.context = context;
    }
}
