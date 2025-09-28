using Ks.Ava.Mvvm.Base.Plugin;
using Ks.Ava.Mvvm.Plugin.Setting.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Ks.Ava.Mvvm.Plugin.Setting;

public class SettingPlugin : IPlugin
{
    public string Name => "Setting";

    public Type ViewModelType => typeof(SettingPageViewModels);
    
    public void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IPlugin, SettingPlugin>();
        services.AddTransient<SettingPageViewModels>();
    }
}