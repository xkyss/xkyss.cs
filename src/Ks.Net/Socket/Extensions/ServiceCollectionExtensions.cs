using Ks.Net.Socket.Client;
using Ks.Net.Socket.Codec;
using Ks.Net.Socket.Server;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;

namespace Ks.Net.Socket.Extensions;

/// <summary>
/// ServiceCollection扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    
    /// <summary>
    /// 添加Socket Client
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureOptions"></param> 
    /// <returns></returns>
    public static IServiceCollection AddSocketClient(this IServiceCollection services, Action<ClientOptions> configureOptions)
    {
        return services.AddSocketClient().Configure(configureOptions);
    }

    /// <summary>
    /// 添加Socket Client
    /// </summary>
    /// <param name="services"></param> 
    /// <returns></returns>
    public static IServiceCollection AddSocketClient(this IServiceCollection services)
    {
        services.AddTransient<SocketClient>();
        services.AddSingleton<ISocketDecoder, MessagePackDecoder>();
        services.AddSingleton<ISocketEncoder, MessagePackEncoder>();
        services.AddSingleton<ISocketTypeMapper, SocketTypeMapper>();
        return services;
    }
    
    /// <summary>
    /// 添加Socket Server
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureOptions"></param> 
    /// <returns></returns>
    public static IServiceCollection AddSocketServer(this IServiceCollection services, Action<ServerOptions> configureOptions)
    {
        return services.AddSocketServer().Configure(configureOptions);
    }

    /// <summary>
    /// 添加Socket Server
    /// </summary>
    /// <param name="services"></param> 
    /// <returns></returns>
    public static IServiceCollection AddSocketServer(this IServiceCollection services)
    {
        services.AddTransient<ServerClient>();
        services.AddSingleton<ISocketDecoder, MessagePackDecoder>();
        services.AddSingleton<ISocketEncoder, MessagePackEncoder>();
        services.AddSingleton<ISocketTypeMapper, SocketTypeMapper>();
        return services;
    }
}