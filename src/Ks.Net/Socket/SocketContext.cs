using Ks.Net.Kestrel;
using Microsoft.AspNetCore.Http.Features;

namespace Ks.Net.Socket;

public class SocketContext<TClient>(TClient client, SocketRequest request, SocketResponse response, IFeatureCollection? features)
    : NetContext(features)
    where TClient : ISocketClient
{
    /// <summary>
    /// 客户端
    /// </summary>
    public TClient Client { get; } = client;
    
    /// <summary>
    /// 请求
    /// </summary>
    public SocketRequest Request { get; } = request;

    /// <summary>
    /// 响应
    /// </summary>
    public SocketResponse Response { get; } = response;
}