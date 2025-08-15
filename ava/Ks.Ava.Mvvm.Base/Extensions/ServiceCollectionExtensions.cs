using System.Reflection;
using Ks.Ava.Mvvm.Base.Plugin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ks.Ava.Mvvm.Base.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册插件
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddPlugins(this IServiceCollection services, IConfiguration configuration)
        {
            var bs = configuration.GetSection(BaseSettings.Tag).Get<BaseSettings>();
            
            var pluginPath = bs?.PluginPath;
            if (!Directory.Exists(pluginPath))
            {
                return;
            }
        
            var files = Directory.GetFiles(pluginPath, "*.dll", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var assembly = Assembly.LoadFrom(file);
                var plugins = assembly.GetTypes()
                        // 所有 IPlugin 接口的非抽象类实现
                        .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract)
                        // 创建实例
                        .Select(t => (IPlugin)Activator.CreateInstance(t)!)
                    ;

                foreach (var plugin in plugins)
                {
                    plugin.RegisterServices(services);
                }
            }
        }


        /// <summary>
        /// 注册全局配置
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddBaseSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BaseSettings>(configuration.GetSection(BaseSettings.Tag));
            return services;
        }
    }
}