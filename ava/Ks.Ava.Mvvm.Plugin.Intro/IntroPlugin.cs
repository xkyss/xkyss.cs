using Ks.Ava.Mvvm.Base.Plugin;
using Ks.Ava.Mvvm.Plugin.Intro.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Ks.Ava.Mvvm.Plugin.Intro;

public class IntroPlugin : IPlugin
{
    public string Name => "Intro";

    public Type ViewModelType => typeof(IntroPageViewModels);
    
    public void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IPlugin, IntroPlugin>();
        services.AddTransient<IntroPageViewModels>();
    }
}