using System.Runtime.CompilerServices;
using System.Text.Json;
using Mewoo.Abstractions.Logging;
using Mewoo.Abstractions.Plugins;

namespace Mewoo.Core.Plugins;

public sealed class MewooRuntimePluginManager
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    private readonly MewooRuntimePluginCatalog _catalog;
    private readonly MewooRuntimePluginFactory _factory;
    private readonly IMewooLogger? _logger;
    private readonly List<MewooLoadedRuntimePlugin> _loadedPlugins = [];
    private readonly List<MewooRuntimePluginStatus> _pluginStatuses = [];
    private readonly List<MewooRuntimePluginIssue> _discoveryIssues = [];
    private readonly Version _currentVersion;

    public MewooRuntimePluginManager(
        IMewooLogger? logger = null,
        MewooRuntimePluginCatalog? catalog = null,
        MewooRuntimePluginFactory? factory = null,
        Version? currentVersion = null)
    {
        _logger = logger;
        _catalog = catalog ?? new MewooRuntimePluginCatalog(logger);
        _factory = factory ?? new MewooRuntimePluginFactory(logger);
        _currentVersion = currentVersion ?? new Version(1, 0, 0);
    }

    public IReadOnlyList<MewooLoadedRuntimePlugin> LoadedPlugins => _loadedPlugins;

    public IReadOnlyList<MewooRuntimePluginStatus> PluginStatuses => _pluginStatuses;

    public IReadOnlyList<MewooRuntimePluginIssue> DiscoveryIssues => _discoveryIssues;

    public IReadOnlyList<MewooLoadedRuntimePlugin> LoadDiscoveredPlugins(string pluginRoot, MewooPluginHost pluginHost)
    {
        var registered = new List<MewooLoadedRuntimePlugin>();
        var descriptors = _catalog.Discover(pluginRoot);
        _discoveryIssues.Clear();
        _discoveryIssues.AddRange(_catalog.DiscoveryIssues);

        foreach (var descriptor in descriptors)
        {
            if (_loadedPlugins.Any(plugin => string.Equals(plugin.Plugin.Id, descriptor.Manifest.Id, StringComparison.Ordinal)))
            {
                continue;
            }

            var loaded = TryRegisterDescriptor(descriptor, pluginHost);
            if (loaded is not null)
            {
                registered.Add(loaded);
            }
        }

        return registered;
    }

    public async ValueTask ActivateLoadedPluginsAsync(
        MewooPluginHost pluginHost,
        Func<IMewooPlugin, IMewooPluginContext> createContext,
        CancellationToken cancellationToken = default)
    {
        foreach (var loaded in _loadedPlugins.ToArray())
        {
            await ActivateRuntimePluginAsync(loaded, pluginHost, createContext, cancellationToken);
        }
    }

    public async ValueTask<bool> UnloadPluginAsync(
        string pluginId,
        MewooPluginHost pluginHost,
        CancellationToken cancellationToken = default)
    {
        var unload = await UnloadAndReleasePluginAsync(pluginId, pluginHost, cancellationToken);
        if (unload is null)
        {
            return false;
        }

        await Task.Yield();
        var collected = VerifyLoadContextCollected(unload.LoadContextReference);
        var currentDescriptor = _pluginStatuses
            .FirstOrDefault(status => string.Equals(status.Descriptor.Manifest.Id, unload.PluginId, StringComparison.Ordinal))
            ?.Descriptor ?? unload.Descriptor;
        SetStatus(
            currentDescriptor,
            currentDescriptor.Manifest.Disabled ? MewooRuntimePluginState.Disabled : MewooRuntimePluginState.Discovered,
            currentDescriptor.Manifest.Disabled ? "Plugin is disabled by manifest." : "Plugin was unloaded.",
            currentDescriptor.Manifest.Disabled
                ? MewooRuntimePluginIssueCategory.Disabled
                : MewooRuntimePluginIssueCategory.None);
        _logger?.Info("RuntimePluginManager", $"Unloaded runtime plugin '{unload.PluginId}'.");
        if (collected)
        {
            _logger?.Info("RuntimePluginManager", $"Runtime plugin '{unload.PluginId}' load context was collected.");
        }
        else
        {
            _logger?.Error("RuntimePluginManager", $"Runtime plugin '{unload.PluginId}' load context is still alive after unload verification.");
        }

        return true;
    }

    private async ValueTask<RuntimePluginUnloadResult?> UnloadAndReleasePluginAsync(
        string pluginId,
        MewooPluginHost pluginHost,
        CancellationToken cancellationToken)
    {
        var loaded = _loadedPlugins.FirstOrDefault(plugin =>
            string.Equals(plugin.Plugin.Id, pluginId, StringComparison.Ordinal));
        if (loaded is null)
        {
            return null;
        }

        await pluginHost.UnloadPluginAsync(pluginId, cancellationToken);
        var descriptor = loaded.Descriptor;
        _loadedPlugins.Remove(loaded);
        pluginHost.ForgetPlugin(pluginId);
        var unloadReference = RequestLoadContextUnload(loaded.LoadContext);

        return new RuntimePluginUnloadResult(pluginId, descriptor, unloadReference);
    }

    public async ValueTask UnloadAllAsync(MewooPluginHost pluginHost, CancellationToken cancellationToken = default)
    {
        foreach (var loaded in _loadedPlugins.ToArray())
        {
            await UnloadPluginAsync(loaded.Plugin.Id, pluginHost, cancellationToken);
        }
    }

    public async ValueTask<bool> ReloadPluginAsync(
        string pluginId,
        string pluginRoot,
        MewooPluginHost pluginHost,
        Func<IMewooPlugin, IMewooPluginContext> createContext,
        CancellationToken cancellationToken = default)
    {
        await UnloadPluginAsync(pluginId, pluginHost, cancellationToken);

        var descriptors = _catalog.Discover(pluginRoot);
        _discoveryIssues.Clear();
        _discoveryIssues.AddRange(_catalog.DiscoveryIssues);
        var descriptor = descriptors
            .FirstOrDefault(item => string.Equals(item.Manifest.Id, pluginId, StringComparison.Ordinal));
        if (descriptor is null)
        {
            _logger?.Error("RuntimePluginManager", $"Runtime plugin '{pluginId}' was not found during reload.");
            return false;
        }

        var loaded = TryRegisterDescriptor(descriptor, pluginHost);
        if (loaded is null)
        {
            return false;
        }

        await ActivateRuntimePluginAsync(loaded, pluginHost, createContext, cancellationToken);
        return pluginHost.Plugins.Any(entry =>
            string.Equals(entry.Plugin.Id, loaded.Plugin.Id, StringComparison.Ordinal)
            && entry.State == MewooPluginState.Activated);
    }

    public bool SetPluginDisabled(string pluginId, bool disabled)
    {
        var status = _pluginStatuses.FirstOrDefault(item =>
            string.Equals(item.Descriptor.Manifest.Id, pluginId, StringComparison.Ordinal));
        if (status is null)
        {
            return false;
        }

        var manifest = status.Descriptor.Manifest with { Disabled = disabled };
        var json = JsonSerializer.Serialize(manifest, JsonOptions);
        File.WriteAllText(status.Descriptor.ManifestPath, json);

        var descriptor = status.Descriptor with { Manifest = manifest };
        SetStatus(
            descriptor,
            disabled ? MewooRuntimePluginState.Disabled : MewooRuntimePluginState.Discovered,
            disabled ? "Plugin is disabled by manifest." : "Plugin is enabled.",
            disabled ? MewooRuntimePluginIssueCategory.Disabled : MewooRuntimePluginIssueCategory.None);
        _logger?.Info(
            "RuntimePluginManager",
            $"{(disabled ? "Disabled" : "Enabled")} runtime plugin '{pluginId}'.");
        return true;
    }

    private MewooLoadedRuntimePlugin? TryRegisterDescriptor(
        MewooRuntimePluginDescriptor descriptor,
        MewooPluginHost pluginHost)
    {
        SetStatus(descriptor, MewooRuntimePluginState.Discovered);

        if (descriptor.Manifest.Disabled)
        {
            SetStatus(
                descriptor,
                MewooRuntimePluginState.Disabled,
                "Plugin is disabled by manifest.",
                MewooRuntimePluginIssueCategory.Disabled);
            return null;
        }

        if (!IsCompatible(descriptor, out var compatibilityMessage))
        {
            SetStatus(
                descriptor,
                MewooRuntimePluginState.Incompatible,
                compatibilityMessage,
                MewooRuntimePluginIssueCategory.Compatibility);
            _logger?.Info("RuntimePluginManager", compatibilityMessage!);
            return null;
        }

        var loaded = _factory.TryCreate(descriptor, out var issue);
        if (loaded is null)
        {
            SetStatus(
                descriptor,
                MewooRuntimePluginState.Failed,
                issue?.Message ?? "Plugin entry point could not be created.",
                issue?.Category ?? MewooRuntimePluginIssueCategory.EntryPoint);
            return null;
        }

        SetStatus(descriptor, MewooRuntimePluginState.Loaded);
        var entry = pluginHost.RegisterPlugin(loaded.Plugin, MewooPluginRegistrationSource.Runtime);
        if (entry.State == MewooPluginState.Failed)
        {
            loaded.LoadContext.Unload();
            SetStatus(
                descriptor,
                MewooRuntimePluginState.Failed,
                entry.Error?.Message,
                MewooRuntimePluginIssueCategory.Registration);
            _logger?.Error(
                "RuntimePluginManager",
                $"Runtime plugin '{loaded.Plugin.Id}' failed registration.",
                entry.Error);
            return null;
        }

        _loadedPlugins.Add(loaded);
        SetStatus(descriptor, MewooRuntimePluginState.Registered);
        _logger?.Info("RuntimePluginManager", $"Registered runtime plugin '{loaded.Plugin.Id}'.");
        return loaded;
    }

    private async ValueTask ActivateRuntimePluginAsync(
        MewooLoadedRuntimePlugin loaded,
        MewooPluginHost pluginHost,
        Func<IMewooPlugin, IMewooPluginContext> createContext,
        CancellationToken cancellationToken)
    {
        await pluginHost.ActivatePluginAsync(loaded.Plugin.Id, createContext(loaded.Plugin), cancellationToken);
        var entry = pluginHost.Plugins.FirstOrDefault(plugin =>
            string.Equals(plugin.Plugin.Id, loaded.Plugin.Id, StringComparison.Ordinal));
        if (entry?.State == MewooPluginState.Failed)
        {
            SetStatus(
                loaded.Descriptor,
                MewooRuntimePluginState.Failed,
                entry.Error?.Message,
                MewooRuntimePluginIssueCategory.Activation);
            return;
        }

        if (entry?.State == MewooPluginState.Activated)
        {
            SetStatus(loaded.Descriptor, MewooRuntimePluginState.Registered, "Plugin is registered and active.");
        }
    }

    private bool IsCompatible(MewooRuntimePluginDescriptor descriptor, out string? message)
    {
        message = null;
        if (string.IsNullOrWhiteSpace(descriptor.Manifest.MinimumMewooVersion))
        {
            return true;
        }

        if (!Version.TryParse(descriptor.Manifest.MinimumMewooVersion, out var minimumVersion))
        {
            message = $"Runtime plugin '{descriptor.Manifest.Id}' has invalid minimumMewooVersion '{descriptor.Manifest.MinimumMewooVersion}'.";
            return false;
        }

        if (minimumVersion <= _currentVersion)
        {
            return true;
        }

        message = $"Runtime plugin '{descriptor.Manifest.Id}' requires Mewoo {minimumVersion} or newer.";
        return false;
    }

    private void SetStatus(
        MewooRuntimePluginDescriptor descriptor,
        MewooRuntimePluginState state,
        string? message = null,
        MewooRuntimePluginIssueCategory issueCategory = MewooRuntimePluginIssueCategory.None)
    {
        var index = _pluginStatuses.FindIndex(status =>
            string.Equals(status.Descriptor.Manifest.Id, descriptor.Manifest.Id, StringComparison.Ordinal));
        var status = new MewooRuntimePluginStatus(descriptor, state, message, issueCategory);
        if (index < 0)
        {
            _pluginStatuses.Add(status);
        }
        else
        {
            _pluginStatuses[index] = status;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference RequestLoadContextUnload(MewooRuntimePluginLoadContext loadContext)
    {
        loadContext.Unload();
        return new WeakReference(loadContext, trackResurrection: false);
    }

    private static bool VerifyLoadContextCollected(WeakReference unloadReference)
    {
        for (var attempt = 0; attempt < 8; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!unloadReference.IsAlive)
            {
                return true;
            }
        }

        return false;
    }

    private sealed record RuntimePluginUnloadResult(
        string PluginId,
        MewooRuntimePluginDescriptor Descriptor,
        WeakReference LoadContextReference);
}
