using Ks.Ava.Mvvm.Base.Plugin;
using Ks.Ava.Mvvm.Plugin.Hello.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Ks.Ava.Mvvm.Plugin.Hello;

public class HelloPlugin : IPlugin
{
    public string Name => "Hello";

    public Type ViewModelType => typeof(HelloPageViewModels);
    
    public void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IPlugin, HelloPlugin>();
        services.AddTransient<HelloPageViewModels>();
    }
}