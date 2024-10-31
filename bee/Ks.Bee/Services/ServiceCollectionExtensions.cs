using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using Ks.Bee.Base.Abstractions.Navigation;
using Ks.Bee.Base.Abstractions.Plugin;
using Ks.Bee.Base.Impl.Navigation;
using Ks.Bee.Base.Models;
using Ks.Bee.Base.Models.Menu;
using Ks.Bee.Services.Impl.Navigation.Commands;
using Ks.Bee.ViewModels;
using Ke.Bee.Localization.Extensions;
using Ke.Bee.Localization.Options;
using Ke.Bee.Localization.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ks.Bee.Services;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddTransient<MainWindowViewModel>();

        services.AddSettings();
        services.AddMenus();

        // 注册视图导航器
        services.AddSingleton<IViewNavigator, DefaultViewNavigator>();

        // 注册命令
        services.AddSingleton<INavigationCommand, PosterGeneratorNavigationCommand>();
        services.AddSingleton<INavigationCommand, DocumentConverterNavigationCommand>();

        services.AddPlugins();

        // 注册本地化
        services.AddLocalization();

        return services;
    }

    /// <summary>
    /// 注册全局配置
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    private static IServiceCollection AddSettings(this IServiceCollection services)
    {
        var appSettings = Options.Create(new AppSettings
        {
            OutputPath = Path.Combine(AppContext.BaseDirectory, "output"),
            PluginPath = Path.Combine(AppContext.BaseDirectory, "Plugins")
        });
        services.AddSingleton(appSettings);
        return services;
    }

    /// <summary>
    /// 注册应用菜单
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    private static IServiceCollection AddMenus(this IServiceCollection services)
    {
        // 从配置文件读取菜单注入到 DI 容器
        var menuItems = JsonSerializer.Deserialize<List<MenuItem>>(
            File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Configs", "menus.json"))
            );
        var menuContext = new MenuConfigurationContext(menuItems);
        services.AddSingleton(menuContext);
        return services;
    }

    /// <summary>
    /// 注册插件服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    private static IServiceCollection AddPlugins(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var appSettings = serviceProvider.GetService<IOptions<AppSettings>>();
        var pluginPath = appSettings?.Value.PluginPath;
        if (!Directory.Exists(pluginPath))
        {
            return services;
        }

        var menuContext = serviceProvider.GetService<MenuConfigurationContext>();

        var files = Directory.GetFiles(pluginPath, "*.dll", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var assembly = Assembly.LoadFrom(file);
            var plugins = assembly.GetTypes()
                // 所有 IPlugin 接口的非抽象类实现
                .Where(t => typeof(PluginBase).IsAssignableFrom(t) && !t.IsAbstract)
                // 创建实例
                .Select(t => (IPlugin)Activator.CreateInstance(t, appSettings)!)
                ;

            foreach (var plugin in plugins)
            {
                plugin.RegisterServices(services);
                plugin.ConfigureMenu(menuContext!);
            }
        }
        return services;
    }

    /// <summary>
    /// 注册本地化
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    private static IServiceCollection AddLocalization(this IServiceCollection services)
    {
        services.AddLocalization<AvaloniaJsonLocalizationProvider>(() =>
        {
            var options = new AvaloniaLocalizationOptions(
                // 支持的本地化语言文化
                [
                    new("en-US"),
                    new("zh-CN")
                ],
                // defaultCulture, 用于设置当前文化（currentCulture）不在 cultures 列表中时的情况以及作为缺失的本地化条目的备用文化（fallback culture）
                new CultureInfo("en-US"),
                // currentCulture 在基础设施加载时设置，可以从应用程序设置或其他地方获取
                Thread.CurrentThread.CurrentCulture,
                // 包含本地化 JSON 文件的资源路径
                $"{typeof(App).Namespace}/Assets/i18n");
            return options;
        });
        return services;
    }
}