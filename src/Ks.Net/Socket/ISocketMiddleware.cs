using Ks.Net.Kestrel;

namespace Ks.Net.Socket;

internal interface ISocketMiddleware<TClient> : INetMiddleware<SocketContext<TClient>>
    where TClient : ISocketClient
{
}
