using Ks.Net.Kestrel;
using Microsoft.AspNetCore.Http.Features;

namespace Ks.Net.SocketServer;

sealed class SocketServerContext : NetContext
{
    public SocketServerContext(IFeatureCollection features) : base(features)
    {
    }
}