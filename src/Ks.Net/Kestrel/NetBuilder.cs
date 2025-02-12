﻿using Microsoft.Extensions.DependencyInjection;

namespace Ks.Net.Kestrel;

/// <summary>
/// 网络创建者
/// </summary>
public class NetBuilder<TContext>
{
    private readonly NetDelegate<TContext> fallbackHandler;
    private readonly List<Func<NetDelegate<TContext>, NetDelegate<TContext>>> middlewares = new();

    /// <summary>
    /// 获取服务提供者
    /// </summary>
    public IServiceProvider ApplicationServices { get; }

    /// <summary>
    /// 应用程序创建者
    /// </summary>
    /// <param name="appServices"></param>
    public NetBuilder(IServiceProvider appServices)
        : this(appServices, context => Task.CompletedTask)
    {
    }

    /// <summary>
    /// 应用程序创建者
    /// </summary>
    /// <param name="appServices"></param>
    /// <param name="fallbackHandler">回退处理者</param>
    public NetBuilder(IServiceProvider appServices, NetDelegate<TContext> fallbackHandler)
    {
        this.ApplicationServices = appServices;
        this.fallbackHandler = fallbackHandler;
    }

    /// <summary>
    /// 创建处理应用请求的委托
    /// </summary>
    /// <returns></returns>
    public NetDelegate<TContext> Build()
    {
        var handler = this.fallbackHandler;
        for (var i = this.middlewares.Count - 1; i >= 0; i--)
        {
            handler = this.middlewares[i](handler);
        }
        return handler;
    }


    /// <summary>
    /// 使用默认配制创建新的PipelineBuilder
    /// </summary>
    /// <returns></returns>
    public NetBuilder<TContext> New()
    {
        return new NetBuilder<TContext>(this.ApplicationServices, this.fallbackHandler);
    }

    /// <summary>
    /// 条件中间件
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="handler"></param> 
    /// <returns></returns>
    public NetBuilder<TContext> When(Func<TContext, bool> predicate, NetDelegate<TContext> handler)
    {
        return this.Use(next => async context =>
        {
            if (predicate(context))
            {
                await handler(context);
            }
            else
            {
                await next(context);
            }
        });
    }


    /// <summary>
    /// 条件中间件
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="configureAction"></param>
    /// <returns></returns>
    public NetBuilder<TContext> When(Func<TContext, bool> predicate, Action<NetBuilder<TContext>> configureAction)
    {
        return this.Use(next => async context =>
        {
            if (predicate(context))
            {
                var branchBuilder = this.New();
                configureAction(branchBuilder);
                await branchBuilder.Build().Invoke(context);
            }
            else
            {
                await next(context);
            }
        });
    }

    /// <summary>
    /// 使用中间件
    /// </summary>
    /// <typeparam name="TMiddleware"></typeparam>
    /// <returns></returns>
    public NetBuilder<TContext> Use<TMiddleware>()
        where TMiddleware : INetMiddleware<TContext>
    {
        var middleware = ActivatorUtilities.GetServiceOrCreateInstance<TMiddleware>(this.ApplicationServices);
        return this.Use(middleware);
    }

    /// <summary>
    /// 使用中间件
    /// </summary>
    /// <typeparam name="TMiddleware"></typeparam>
    /// <returns></returns>
    public NetBuilder<TContext> Use<TMiddleware>(Action<TMiddleware> initializer)
        where TMiddleware : INetMiddleware<TContext>
    {
        var middleware = ActivatorUtilities.GetServiceOrCreateInstance<TMiddleware>(this.ApplicationServices);
        initializer.Invoke(middleware);
        return this.Use(middleware.InvokeAsync);
    }

    /// <summary>
    /// 使用中间件
    /// </summary> 
    /// <typeparam name="TMiddleware"></typeparam> 
    /// <param name="middleware"></param>
    /// <returns></returns>
    public NetBuilder<TContext> Use<TMiddleware>(TMiddleware middleware)
        where TMiddleware : INetMiddleware<TContext>
    {
        return this.Use(middleware.InvokeAsync);
    }

    /// <summary>
    /// 使用中间件
    /// </summary>  
    /// <param name="middleware"></param>
    /// <returns></returns>
    public NetBuilder<TContext> Use(Func<NetDelegate<TContext>, TContext, Task> middleware)
    {
        return this.Use(next => context => middleware(next, context));
    }

    /// <summary>
    /// 使用中间件
    /// </summary>
    /// <param name="middleware"></param>
    /// <returns></returns>
    public NetBuilder<TContext> Use(Func<NetDelegate<TContext>, NetDelegate<TContext>> middleware)
    {
        this.middlewares.Add(middleware);
        return this;
    }
}
