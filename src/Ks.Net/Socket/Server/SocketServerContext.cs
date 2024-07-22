using Ks.Net.Kestrel;
using Microsoft.AspNetCore.Http.Features;

namespace Ks.Net.Socket;

/// <summary>
/// 上下文信息
/// </summary>
sealed class SocketServerContext : SocketContext
{
    /// <summary>
    /// 请求的客户端
    /// </summary>
    public ConnectedClient Client { get; }

    public SocketServerContext(ConnectedClient client, SocketRequest request, SocketResponse response, IFeatureCollection features)
        : base(request, response, features)
    {
        Client = client;
    }
}