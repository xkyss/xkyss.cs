using Aprillz.MewUI;
using Mewoo.Abstractions;
using Mewoo.Abstractions.Plugins;
using Mewoo.Core.Plugins;

namespace Mewoo.Plugins.PluginManager;

internal sealed class PluginManagerController
{
    private readonly PluginManagerDependencies _dependencies;
    private readonly PluginManagerSession _session;

    public PluginManagerController(
        PluginManagerDependencies dependencies,
        PluginManagerSession session)
    {
        _dependencies = dependencies;
        _session = session;
    }

    public IReadOnlyList<MewooPluginManagerCatalogEntry> Entries() =>
        FilterAttentionEntries(_dependencies.Catalog.CreateEntries(
            _dependencies.PluginRoot,
            _dependencies.RuntimePlugins.PluginStatuses,
            _dependencies.RuntimePlugins.DiscoveryIssues,
            new MewooPluginManagerCatalogQuery(_session.SearchText, CatalogFilter())));

    public IReadOnlyList<MewooRuntimePluginStatus> RuntimeStatuses =>
        _dependencies.RuntimePlugins.PluginStatuses;

    public IReadOnlyList<MewooRuntimePluginIssue> DiscoveryIssues =>
        _dependencies.RuntimePlugins.DiscoveryIssues;

    public int RuntimeStatusCount => _dependencies.RuntimePlugins.PluginStatuses.Count;

    public int DiscoveryIssueCount => _dependencies.RuntimePlugins.DiscoveryIssues.Count;

    public int RuntimeProblemCount => _dependencies.RuntimePlugins.PluginStatuses.Count(status =>
        status.State is MewooRuntimePluginState.Incompatible or MewooRuntimePluginState.Failed);

    public IReadOnlyList<MewooPluginCatalogEntry> DiagnosticCatalogEntries() =>
        MewooPluginCatalogDisplay.CreateEntries(
            _dependencies.RuntimePlugins.PluginStatuses,
            _dependencies.RuntimePlugins.DiscoveryIssues);

    public IReadOnlyList<Mewoo.Abstractions.Logging.MewooLogEntry> RuntimeLogs() =>
        _dependencies.Logger?.Entries
            .Where(entry => entry.Source.StartsWith("RuntimePlugin", StringComparison.Ordinal))
            .TakeLast(25)
            .ToArray()
        ?? [];

    public MewooPluginManagerCatalogEntry? SelectedEntry()
    {
        if (string.IsNullOrWhiteSpace(_session.SelectedEntryKey))
        {
            return null;
        }

        return _dependencies.Catalog.CreateEntries(
                _dependencies.PluginRoot,
                _dependencies.RuntimePlugins.PluginStatuses,
                _dependencies.RuntimePlugins.DiscoveryIssues)
            .FirstOrDefault(entry => string.Equals(
                PluginManagerFormatting.EntryKey(entry),
                _session.SelectedEntryKey,
                StringComparison.Ordinal));
    }

    public async Task ChooseInstallPackageAsync(IWorkbenchService workbench)
    {
        var packagePath = FileDialog.OpenFile(new OpenFileDialogOptions
        {
            Title = "Install Mewoo Plugin",
            Filter = $"Mewoo Plugin (*{MewooPluginPackageFormat.Extension})|*{MewooPluginPackageFormat.Extension}",
        });
        if (string.IsNullOrWhiteSpace(packagePath))
        {
            return;
        }

        _session.InstallPreview = _dependencies.InstallPreviewer.Preview(packagePath);
        await workbench.OpenMainViewAsync("pluginManager.installPreview");
    }

    public async Task ChooseUpdatePackageAsync(
        MewooPluginManagerCatalogEntry entry,
        IWorkbenchService workbench)
    {
        if (string.IsNullOrWhiteSpace(entry.PluginId))
        {
            _session.AddOperation($"Update failed: {entry.DisplayName} does not have a readable plugin id.");
            await workbench.OpenMainViewAsync("pluginManager.details");
            return;
        }

        var packagePath = FileDialog.OpenFile(new OpenFileDialogOptions
        {
            Title = $"Update {entry.DisplayName}",
            Filter = $"Mewoo Plugin (*{MewooPluginPackageFormat.Extension})|*{MewooPluginPackageFormat.Extension}",
        });
        if (string.IsNullOrWhiteSpace(packagePath))
        {
            return;
        }

        _session.SelectedEntryKey = PluginManagerFormatting.EntryKey(entry);
        _session.UpdatePreview = _dependencies.UpdatePreviewer.Preview(packagePath, entry);
        await workbench.OpenMainViewAsync("pluginManager.updatePreview");
    }

    public async Task<bool> InstallPreviewedPackageAsync(IWorkbenchService workbench)
    {
        if (_session.InstallPreview is not { Success: true } preview)
        {
            _session.AddOperation("Install failed: no valid package preview.");
            return false;
        }

        var result = _dependencies.Installer.Install(preview.PackagePath, _dependencies.PluginRoot);
        var summary = MewooPluginInstallFlowDisplay.CreateResultSummary(result);
        _session.AddOperation(summary.Success ? summary.Message : $"Install failed: {summary.Message}");
        if (summary.Success && summary.PluginId is not null)
        {
            await _dependencies.RuntimePlugins.ReloadPluginAsync(
                summary.PluginId,
                _dependencies.PluginRoot,
                _dependencies.PluginHost,
                plugin => new PluginManagerPluginContext(plugin.Id, _dependencies.Services, workbench));
            _session.SelectedEntryKey = summary.PluginId;
            _session.InstallPreview = null;
            await workbench.OpenMainViewAsync("pluginManager.details");
            return true;
        }

        return false;
    }

    public async Task<bool> UpdatePreviewedPackageAsync(IWorkbenchService workbench)
    {
        if (_session.UpdatePreview is not { Success: true } preview)
        {
            _session.AddOperation("Update failed: no valid package preview.");
            return false;
        }

        var result = await _dependencies.PackageOperations.UpdateAsync(
            preview.PackagePath,
            _dependencies.PluginRoot,
            _dependencies.RuntimePlugins,
            _dependencies.PluginHost,
            plugin => new PluginManagerPluginContext(plugin.Id, _dependencies.Services, workbench));
        var summary = MewooPluginUpdateFlowDisplay.CreateResultSummary(result);
        _session.AddOperation(summary.Success ? summary.Message : $"Update failed: {summary.Message}");
        if (summary.Success && summary.PluginId is not null)
        {
            _session.SelectedEntryKey = summary.PluginId;
            _session.UpdatePreview = null;
            await workbench.OpenMainViewAsync("pluginManager.details");
            return true;
        }

        return false;
    }

    public async Task EnableAsync(
        MewooPluginManagerCatalogEntry entry,
        IWorkbenchService workbench)
    {
        if (entry.PluginId is null)
        {
            return;
        }

        if (_dependencies.RuntimePlugins.SetPluginDisabled(entry.PluginId, disabled: false))
        {
            await _dependencies.RuntimePlugins.ReloadPluginAsync(
                entry.PluginId,
                _dependencies.PluginRoot,
                _dependencies.PluginHost,
                plugin => new PluginManagerPluginContext(plugin.Id, _dependencies.Services, workbench));
            _session.AddOperation($"Enabled {entry.DisplayName}");
        }
    }

    public async Task DisableAsync(MewooPluginManagerCatalogEntry entry)
    {
        if (entry.PluginId is null)
        {
            return;
        }

        if (_dependencies.RuntimePlugins.SetPluginDisabled(entry.PluginId, disabled: true))
        {
            await _dependencies.RuntimePlugins.UnloadPluginAsync(entry.PluginId, _dependencies.PluginHost);
            _session.AddOperation($"Disabled {entry.DisplayName}");
        }
    }

    public async Task RemoveBrokenInstallAsync(MewooPluginManagerCatalogEntry entry)
    {
        var result = await _dependencies.PackageOperations.RemoveBrokenInstallAsync(
            entry,
            _dependencies.PluginRoot);
        _session.AddOperation(result.Success
            ? $"Removed broken install {entry.DisplayName}"
            : $"Remove broken install failed: {result.Issue?.ShortMessage ?? "Unknown error"}");
        _session.SelectedEntryKey = null;
    }

    public async Task UninstallAsync(MewooPluginManagerCatalogEntry entry)
    {
        if (entry.PluginId is null)
        {
            return;
        }

        var result = await _dependencies.PackageOperations.UninstallAsync(
            entry.PluginId,
            _dependencies.PluginRoot,
            _dependencies.RuntimePlugins,
            _dependencies.PluginHost);
        _session.AddOperation(result.Success
            ? $"Uninstalled {entry.DisplayName}"
            : $"Uninstall failed: {result.Issue?.ShortMessage ?? "Unknown error"}");
        _session.SelectedEntryKey = null;
    }

    public async Task OpenDiagnosticsAsync(IWorkbenchService workbench) =>
        await workbench.OpenMainViewAsync("pluginManager.runtimeStatus");

    public async Task ReloadRuntimePluginAsync(
        MewooRuntimePluginStatus status,
        IWorkbenchService workbench)
    {
        var pluginId = status.Descriptor.Manifest.Id;
        await _dependencies.RuntimePlugins.ReloadPluginAsync(
            pluginId,
            _dependencies.PluginRoot,
            _dependencies.PluginHost,
            plugin => new PluginManagerPluginContext(plugin.Id, _dependencies.Services, workbench));
        _session.AddOperation($"Reloaded {status.Descriptor.Manifest.DisplayName}");
        _dependencies.Logger?.Info("RuntimePlugin.PackageOperation", $"Reloaded plugin '{pluginId}'.");
    }

    public async Task UnloadRuntimePluginAsync(MewooRuntimePluginStatus status)
    {
        var pluginId = status.Descriptor.Manifest.Id;
        await _dependencies.RuntimePlugins.UnloadPluginAsync(pluginId, _dependencies.PluginHost);
        _session.AddOperation($"Unloaded {status.Descriptor.Manifest.DisplayName}");
        _dependencies.Logger?.Info("RuntimePlugin.PackageOperation", $"Unloaded plugin '{pluginId}'.");
    }

    private MewooPluginManagerCatalogFilter CatalogFilter() =>
        _session.Category switch
        {
            PluginManagerCategory.Enabled => MewooPluginManagerCatalogFilter.Enabled,
            PluginManagerCategory.Disabled => MewooPluginManagerCatalogFilter.Disabled,
            _ => MewooPluginManagerCatalogFilter.All,
        };

    private IReadOnlyList<MewooPluginManagerCatalogEntry> FilterAttentionEntries(
        IReadOnlyList<MewooPluginManagerCatalogEntry> entries)
    {
        if (_session.Category != PluginManagerCategory.NeedsAttention)
        {
            return entries;
        }

        return entries
            .Where(entry => entry.State is
                MewooPluginManagerCatalogState.Failed or
                MewooPluginManagerCatalogState.Incompatible or
                MewooPluginManagerCatalogState.Broken)
            .ToArray();
    }

    private sealed record PluginManagerPluginContext(
        string PluginId,
        IServiceProvider Services,
        IWorkbenchService Workbench) : IMewooPluginContext;
}
