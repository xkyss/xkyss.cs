using Ava.Mvvm.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Ava.Mvvm.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection) {
        collection.AddTransient<MainViewViewModel>();
        collection.AddTransient<MainWindowViewModel>();
        collection.AddTransient<MenuItemViewModel>();
        collection.AddTransient<MenuViewModel>();
        
        collection.AddTransient<IntroPageViewModel>();
        collection.AddTransient<SettingsPageViewModel>();
    } 
}