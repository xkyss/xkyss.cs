using Aprillz.MewUI;
using Mewoo.Abstractions;
using Mewoo.Core.Plugins;
using Mewoo.Plugins.QuickLauncher;
using Mewoo.Workbench;

Startup();

var pluginHost = new MewooPluginHost();
pluginHost.RegisterPlugin(new QuickLauncherPlugin());
var themeController = new MewooThemeController();

MewooWorkbenchWindow? window = null;
Application
    .Create()
    .UseAccent(Accent.Purple)
    .BuildMainWindow(() =>
    {
        window = new MewooWorkbenchWindow(pluginHost, new EmptyServiceProvider(), themeController);
        window.Loaded += async () =>
        {
            themeController.Apply(Mewoo.Abstractions.Theming.MewooBuiltInThemes.DarkId);
            await pluginHost.ActivateAllAsync(
                plugin => new PluginContext(plugin.Id, new EmptyServiceProvider(), window));
            if (pluginHost.Commands.Contains("quickLauncher.open"))
            {
                await pluginHost.Commands.ExecuteAsync(
                    "quickLauncher.open",
                    new MewooCommandContext(new EmptyServiceProvider(), window));
            }
        };
        return window;
    })
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

internal sealed record PluginContext(
    string PluginId,
    IServiceProvider Services,
    IWorkbenchService Workbench) : Mewoo.Abstractions.Plugins.IMewooPluginContext;
