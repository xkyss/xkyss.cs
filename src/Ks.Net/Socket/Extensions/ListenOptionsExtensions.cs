using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Ks.Net.Socket.Extensions;

/// <summary>
///  ListenOptions扩展
/// </summary>
public static class ListenOptionsExtensions
{
    /// <summary>
    /// 使用RedisConnectionHandler
    /// </summary>
    /// <param name="listen"></param>
    public static void UseSocketServer(this ListenOptions listen)
    {
        listen.UseConnectionHandler<ServerConnectionHandler>();
    }
}