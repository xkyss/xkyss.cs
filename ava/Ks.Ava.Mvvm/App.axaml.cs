using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Ks.Ava.Mvvm.Base.Extensions;
using Ks.Ava.Mvvm.Extensions;
using Avalonia.Markup.Xaml;
using Ks.Ava.Mvvm.ViewModels;
using Ks.Ava.Mvvm.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ks.Ava.Mvvm;

public partial class App : Application
{
    private IHost _host;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 如果使用 CommunityToolkit，则需要用下面一行移除 Avalonia 数据验证。
        // 如果没有这一行，数据验证将会在 Avalonia 和 CommunityToolkit 中重复。
        BindingPlugins.DataValidators.RemoveAt(0);

        _host = Host.CreateDefaultBuilder()
            // 配置
            .ConfigureAppConfiguration((context, builder) =>
            {
                var env = context.HostingEnvironment;
                var contentRootPath = context.HostingEnvironment.ContentRootPath;
                
                builder.AddJsonFile("appsettings.json", true);
                builder.AddJsonFile(Path.Combine(contentRootPath, "appsettings.json"), true);
                builder.AddJsonFile(Path.Combine(contentRootPath, $"appsettings.{env.EnvironmentName}.json"), true);
            })
            // 注册应用程序运行所需的服务
            .ConfigureServices((context, services) =>
            {
                services.AddCommonServices();
                services.AddBaseSettings(context.Configuration);
                services.AddPlugins(context.Configuration);
            })
            .Build();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = _host.Services.GetRequiredService<MainViewViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}