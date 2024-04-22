using Microsoft.Extensions.DependencyInjection;

namespace Ks.Net.SocketServer.Extensions;

/// <summary>
/// ServiceCollection扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加Redis
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureOptions"></param> 
    /// <returns></returns>
    public static IServiceCollection AddSocketServer(this IServiceCollection services, Action<ServerOptions> configureOptions)
    {
        return services.AddSocketServer().Configure(configureOptions);
    }

    /// <summary>
    /// 添加Redis
    /// </summary>
    /// <param name="services"></param> 
    /// <returns></returns>
    public static IServiceCollection AddSocketServer(this IServiceCollection services)
    {
        return services;
    }
}