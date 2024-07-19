using Ks.Net.Kestrel;
using Microsoft.AspNetCore.Http.Features;

namespace Ks.Net.Socket;

/// <summary>
/// 上下文信息
/// </summary>
sealed class SocketServerContext : NetContext
{
    /// <summary>
    /// 请求的客户端
    /// </summary>
    public ConnectedClient Client { get; }

    /// <summary>
    /// 请求
    /// </summary>
    public SocketRequest Request { get; }

    /// <summary>
    /// 响应
    /// </summary>
    public SocketResponse Response { get; }

    public SocketServerContext(ConnectedClient client, SocketRequest request, SocketResponse response, IFeatureCollection features) 
        : base(features)
    {
        Client = client;
        Request = request;
        Response = response;
    }
}