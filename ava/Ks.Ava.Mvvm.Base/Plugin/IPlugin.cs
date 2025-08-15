using Microsoft.Extensions.DependencyInjection;

namespace Ks.Ava.Mvvm.Base.Plugin
{
    public interface IPlugin
    {
        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="services"></param>
        void RegisterServices(IServiceCollection services);
    }
}