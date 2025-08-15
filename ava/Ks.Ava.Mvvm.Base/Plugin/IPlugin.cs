using Microsoft.Extensions.DependencyInjection;

namespace Ks.Ava.Mvvm.Base.Plugin
{
    public interface IPlugin
    {
        public string Name { get; }
        
        public Type ViewModelType { get; }
        
        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="services"></param>
        void RegisterServices(IServiceCollection services);
    }
}