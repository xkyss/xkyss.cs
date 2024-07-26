using Ks.Net.Kestrel;

namespace Ks.Net.Socket;

internal interface ISocketMiddleware : INetMiddleware<SocketContext>
{
}
