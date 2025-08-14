using Ks.Ava.Mvvm.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Ks.Ava.Mvvm.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection services) {
        services.AddTransient<MainViewViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MenuItemViewModel>();
        services.AddTransient<MenuViewModel>();
        
        services.AddTransient<IntroPageViewModel>();
        services.AddTransient<SettingsPageViewModel>();
    }
}