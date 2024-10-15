namespace Ks.Net.Kestrel;


/// <summary>
/// 应用程序中间件的接口
/// </summary>
/// <typeparam name="TContext"></typeparam>
public interface INetMiddleware<TContext>
{
    /// <summary>
    /// 执行中间件
    /// </summary>
    /// <param name="next">下一个中间件</param>
    /// <param name="context">上下文</param>
    /// <returns></returns>
    Task InvokeAsync(NetDelegate<TContext> next, TContext context);
}