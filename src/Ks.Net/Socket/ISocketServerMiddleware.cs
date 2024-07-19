using Ks.Net.Kestrel;

namespace Ks.Net.Socket;

internal interface ISocketServerMiddleware : INetMiddleware<SocketServerContext>
{
}
