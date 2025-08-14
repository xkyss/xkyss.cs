using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ks.Ava.Mvvm.Base.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册插件
        /// </summary>
        /// <param name="services"></param>
        public static void AddPlugins(this IServiceCollection services)
        {
            services.AddSingleton<PluginLoader>();
        }
        
        
        /// <summary>
        /// 注册全局配置
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSettings(this IServiceCollection services)
        {
            var appSettings = Options.Create(new BaseSettings
            {
                OutputPath = Path.Combine(AppContext.BaseDirectory, "output"),
                PluginPath = Path.Combine(AppContext.BaseDirectory, "Plugins")
            });
            services.AddSingleton(appSettings);
            return services;
        }
    }
}