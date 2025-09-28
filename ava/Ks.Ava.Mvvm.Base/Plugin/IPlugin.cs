using Microsoft.Extensions.DependencyInjection;

namespace Ks.Ava.Mvvm.Base.Plugin
{
    public interface IPlugin
    {
        /// <summary>
        /// 插件名
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// 插件ViewModel类型
        /// </summary>
        public Type ViewModelType { get; }
        
        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="services"></param>
        void RegisterServices(IServiceCollection services);
    }
}