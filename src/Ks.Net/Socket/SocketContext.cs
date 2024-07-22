using Ks.Net.Kestrel;
using Microsoft.AspNetCore.Http.Features;

namespace Ks.Net.Socket;

internal class SocketContext(SocketRequest request, SocketResponse response, IFeatureCollection features)
    : NetContext(features)
{
    /// <summary>
    /// 请求
    /// </summary>
    public SocketRequest Request { get; } = request;

    /// <summary>
    /// 响应
    /// </summary>
    public SocketResponse Response { get; } = response;
}