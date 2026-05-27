using Mewoo.Abstractions.Logging;
using Mewoo.Abstractions.Plugins;

namespace Mewoo.Core.Plugins;

public sealed class MewooRuntimePluginManager
{
    private readonly MewooRuntimePluginCatalog _catalog;
    private readonly MewooRuntimePluginFactory _factory;
    private readonly IMewooLogger? _logger;
    private readonly List<MewooLoadedRuntimePlugin> _loadedPlugins = [];

    public MewooRuntimePluginManager(
        IMewooLogger? logger = null,
        MewooRuntimePluginCatalog? catalog = null,
        MewooRuntimePluginFactory? factory = null)
    {
        _logger = logger;
        _catalog = catalog ?? new MewooRuntimePluginCatalog(logger);
        _factory = factory ?? new MewooRuntimePluginFactory(logger);
    }

    public IReadOnlyList<MewooLoadedRuntimePlugin> LoadedPlugins => _loadedPlugins;

    public IReadOnlyList<MewooLoadedRuntimePlugin> LoadDiscoveredPlugins(string pluginRoot, MewooPluginHost pluginHost)
    {
        var registered = new List<MewooLoadedRuntimePlugin>();
        foreach (var descriptor in _catalog.Discover(pluginRoot))
        {
            var loaded = _factory.TryCreate(descriptor);
            if (loaded is null)
            {
                continue;
            }

            var entry = pluginHost.RegisterPlugin(loaded.Plugin);
            if (entry.State == MewooPluginState.Failed)
            {
                loaded.LoadContext.Unload();
                _logger?.Error(
                    "RuntimePluginManager",
                    $"Runtime plugin '{loaded.Plugin.Id}' failed registration.",
                    entry.Error);
                continue;
            }

            _loadedPlugins.Add(loaded);
            registered.Add(loaded);
            _logger?.Info("RuntimePluginManager", $"Registered runtime plugin '{loaded.Plugin.Id}'.");
        }

        return registered;
    }

    public async ValueTask<bool> UnloadPluginAsync(
        string pluginId,
        MewooPluginHost pluginHost,
        CancellationToken cancellationToken = default)
    {
        var loaded = _loadedPlugins.FirstOrDefault(plugin =>
            string.Equals(plugin.Plugin.Id, pluginId, StringComparison.Ordinal));
        if (loaded is null)
        {
            return false;
        }

        await pluginHost.UnloadPluginAsync(pluginId, cancellationToken);
        _loadedPlugins.Remove(loaded);
        loaded.LoadContext.Unload();
        _logger?.Info("RuntimePluginManager", $"Unloaded runtime plugin '{pluginId}'.");
        return true;
    }

    public async ValueTask UnloadAllAsync(MewooPluginHost pluginHost, CancellationToken cancellationToken = default)
    {
        foreach (var loaded in _loadedPlugins.ToArray())
        {
            await UnloadPluginAsync(loaded.Plugin.Id, pluginHost, cancellationToken);
        }
    }
}
