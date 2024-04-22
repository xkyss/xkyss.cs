using Ks.Net.Kestrel;

namespace Ks.Net.SocketServer;

internal interface ISocketServerMiddleware : INetMiddleware<SocketServerContext>
{
}
