using Ks.Net.Kestrel;
using Ks.Net.Socket.Client;
using Ks.Net.Socket.Client.Middlewares;
using Ks.Net.Socket.Codec;
using Ks.Net.Socket.MessageHandlers;
using Ks.Net.Socket.Middlewares;
using Ks.Net.Socket.Server;
using Ks.Net.Socket.Telnet;
using Ks.Net.Socket.Telnet.Middlewares;
using Ks.Net.Socket.WebSocketServer;
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
        services.AddTransient(sp => new NetBuilder<SocketContext>(sp)
            .Use<ClientFallbackMiddleware>()
            .Build());
        services.AddTransient<ISocketClient, SocketClient>();
        services.AddInternal();
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
        services.AddTransient(sp => new NetBuilder<SocketContext>(sp)
            .Use<RequestHandlerMiddleware>(middleware =>
            {
                middleware.Register<HeartBeat>(sp.GetRequiredService<HeartBeatHandler>());
            })
            .Use<FallbackMiddleware>()
            .Build());
        
        services.AddSingleton<HeartBeatHandler>();
        services.AddTransient<ServerClient>();
        services.AddInternal();
        return services;
    }

    /// <summary>
    /// 添加Telnet Server
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureOptions"></param> 
    /// <returns></returns>
    public static IServiceCollection AddTelnetServer(this IServiceCollection services, Action<ServerOptions> configureOptions)
    {
        return services.AddTelnetServer().Configure(configureOptions);
    }

    /// <summary>
    /// 添加Socket Server
    /// </summary>
    /// <param name="services"></param> 
    /// <returns></returns>
    public static IServiceCollection AddTelnetServer(this IServiceCollection services)
    {
        services.AddTransient(sp => new NetBuilder<SocketContext>(sp)
            .Use<EmptyMiddleware>()
            .Use<ByeMiddleware>()
            .Use<EchoMiddleware>()
            .Build());
        
        services.AddTransient<TelnetClient>();
        return services;
    }

    private static IServiceCollection AddInternal(this IServiceCollection services)
    {
        services.AddSingleton<ISocketDecoder, MessagePackDecoder>();
        services.AddSingleton<ISocketEncoder, MessagePackEncoder>();
        services.AddSingleton<ISocketTypeMapper, SocketTypeMapper>(sp =>
        {
            var stm = new SocketTypeMapper();
            stm.Register<HeartBeat>(1001);
            return stm;
        });
        return services;
    }
}