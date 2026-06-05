using Mewoo.Core.Plugins;

namespace Mewoo.Plugins.PluginManager;

internal sealed class PluginManagerDependencies
{
    public PluginManagerDependencies(
        MewooRuntimePluginManager runtimePlugins,
        MewooPluginHost pluginHost,
        IServiceProvider services,
        string pluginRoot)
    {
        RuntimePlugins = runtimePlugins;
        PluginHost = pluginHost;
        Services = services;
        PluginRoot = pluginRoot;
    }

    public MewooRuntimePluginManager RuntimePlugins { get; }

    public MewooPluginHost PluginHost { get; }

    public IServiceProvider Services { get; }

    public string PluginRoot { get; }

    public MewooPluginManagerCatalog Catalog { get; } = new();

    public MewooPluginPackageOperations PackageOperations { get; } = new();

    public MewooPluginInstaller Installer { get; } = new();

    public MewooPluginInstallPreviewer InstallPreviewer { get; } = new();

    public MewooPluginUpdatePreviewer UpdatePreviewer { get; } = new();
}
