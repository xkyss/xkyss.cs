using Aprillz.MewUI;
using Mewoo.Abstractions;
using Mewoo.Core.Logging;
using Mewoo.Core.Plugins;
using Mewoo.Core.Storage;
using Mewoo.Plugins.QuickLauncher;
using Mewoo.Workbench;

var logger = new InMemoryMewooLogger();
Startup(logger);
DiscoverRuntimePlugins(logger);
var pluginHost = new MewooPluginHost(logger);
pluginHost.RegisterPlugin(new QuickLauncherPlugin());
var themeController = new MewooThemeController();
var stateStorage = new JsonFileStateStorage(GetStateDirectory());

MewooWorkbenchWindow? window = null;
Application
    .Create()
    .UseAccent(Accent.Purple)
    .BuildMainWindow(() =>
    {
        window = new MewooWorkbenchWindow(pluginHost, new EmptyServiceProvider(), themeController, stateStorage, logger);
        window.Loaded += async () =>
        {
            await pluginHost.ActivateAllAsync(
                plugin => new PluginContext(plugin.Id, new EmptyServiceProvider(), window));
            var snapshot = await stateStorage.ReadJsonAsync<WorkbenchStateSnapshot>("workbench");
            await window.RestoreStateAsync(snapshot);
            if ((snapshot is null || snapshot.OpenMainViewIds.Count == 0)
                && pluginHost.Commands.Contains("quickLauncher.open"))
            {
                await pluginHost.Commands.ExecuteAsync(
                    "quickLauncher.open",
                    new MewooCommandContext(new EmptyServiceProvider(), window));
            }
        };
        return window;
    })
    .Run();

static string GetStateDirectory()
{
    var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    return Path.Combine(root, "Mewoo", "State");
}

static string GetRuntimePluginDirectory()
{
    var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    return Path.Combine(root, "Mewoo", "Plugins");
}

static void DiscoverRuntimePlugins(InMemoryMewooLogger logger)
{
    var pluginDirectory = GetRuntimePluginDirectory();
    Directory.CreateDirectory(pluginDirectory);

    var catalog = new MewooRuntimePluginCatalog(logger);
    _ = catalog.Discover(pluginDirectory);
}

static void Startup(InMemoryMewooLogger logger)
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
        logger.Error("Application", "Unhandled UI exception.", e.Exception);
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
