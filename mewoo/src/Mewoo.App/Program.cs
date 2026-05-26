using Aprillz.MewUI;
using Mewoo.Core.Plugins;
using Mewoo.Plugins.QuickLauncher;
using Mewoo.Workbench;

Startup();

var pluginHost = new MewooPluginHost();
pluginHost.RegisterPlugin(new QuickLauncherPlugin());

Application
    .Create()
    .UseAccent(Accent.Purple)
    .BuildMainWindow(() => new MewooWorkbenchWindow(pluginHost, new EmptyServiceProvider()))
    .Run();

static void Startup()
{
    if (OperatingSystem.IsWindows())
    {
        Win32Platform.Register();
        Direct2DBackend.Register();
    }
    else if (OperatingSystem.IsMacOS())
    {
        MacOSPlatform.Register();
        MewVGMacOSBackend.Register();
    }
    else if (OperatingSystem.IsLinux())
    {
        X11Platform.Register();
        MewVGX11Backend.Register();
    }

    Application.DispatcherUnhandledException += e =>
    {
        Console.Error.WriteLine(e.Exception);
        e.Handled = true;
    };
}

internal sealed class EmptyServiceProvider : IServiceProvider
{
    public object? GetService(Type serviceType) => null;
}
