using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Mewoo.Abstractions;
using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Plugins;
using Mewoo.Abstractions.Views;
using Mewoo.Core.Plugins;

namespace Mewoo.Plugins.PluginManager;

public sealed class PluginManagerPlugin : IMewooPlugin
{
    private readonly MewooRuntimePluginManager _runtimePlugins;
    private readonly MewooPluginHost _pluginHost;
    private readonly IServiceProvider _services;
    private readonly string _pluginRoot;
    private readonly MewooPluginManagerCatalog _catalog = new();
    private readonly MewooPluginPackageOperations _packageOperations = new();
    private readonly MewooPluginInstaller _installer = new();
    private readonly MewooPluginInstallPreviewer _installPreviewer = new();
    private readonly MewooPluginUpdatePreviewer _updatePreviewer = new();
    private readonly List<string> _operationResults = [];

    private string _searchText = string.Empty;
    private MewooPluginManagerCatalogFilter _filter = MewooPluginManagerCatalogFilter.All;
    private string? _selectedEntryKey;
    private MewooPluginInstallPreview? _installPreview;
    private MewooPluginUpdatePreview? _updatePreview;

    public PluginManagerPlugin(
        MewooRuntimePluginManager runtimePlugins,
        MewooPluginHost pluginHost,
        IServiceProvider services,
        string pluginRoot)
    {
        _runtimePlugins = runtimePlugins;
        _pluginHost = pluginHost;
        _services = services;
        _pluginRoot = pluginRoot;
    }

    public string Id => "pluginManager";

    public string DisplayName => "Plugin Manager";

    public void Register(IMewooContributionRegistry registry)
    {
        registry.Activity("pluginManager.activity")
            .Title("Plugins")
            .Icon("P")
            .ViewContainer("pluginManager.views")
            .Order(80);

        registry.ViewContainer("pluginManager.views")
            .Title("Plugins")
            .AddView("pluginManager.installed", view => view
                .Title("Installed")
                .Create(ctx => new MewooView("pluginManager.installed", CreateSidebar(ctx.Workbench))));

        registry.MainView("pluginManager.home")
            .Title("Plugins")
            .CanOpenMultiple(false)
            .Create(ctx => new MewooView("pluginManager.home", CreateMainView(ctx.Workbench)));

        registry.MainView("pluginManager.details")
            .Title("Plugin Details")
            .CanOpenMultiple(false)
            .Create(ctx => new MewooView("pluginManager.details", CreateDetailsView(ctx.Workbench)));

        registry.MainView("pluginManager.installPreview")
            .Title("Install Plugin")
            .CanOpenMultiple(false)
            .Create(ctx => new MewooView("pluginManager.installPreview", CreateInstallPreviewView(ctx.Workbench)));

        registry.MainView("pluginManager.updatePreview")
            .Title("Update Plugin")
            .CanOpenMultiple(false)
            .Create(ctx => new MewooView("pluginManager.updatePreview", CreateUpdatePreviewView(ctx.Workbench)));

        registry.Command("pluginManager.open")
            .Title("Open Plugin Manager")
            .Category("Plugins")
            .Execute(async (ctx, cancellationToken) =>
                await ctx.Workbench.OpenMainViewAsync("pluginManager.home", cancellationToken));
    }

    private StackPanel CreateSidebar(IWorkbenchService workbench)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(8)
            .Margin(12);

        var entries = Entries();
        panel.Children(
            new TextBlock().Text("Plugins").SemiBold(),
            new TextBlock().Text($"{entries.Count} installed").FontSize(12),
            new Button()
                .Content("Open Plugin Manager")
                .OnClick(async () => await workbench.OpenMainViewAsync("pluginManager.home")));

        foreach (var entry in entries.Take(8))
        {
            panel.Children(SidebarEntry(entry, workbench));
        }

        return panel;
    }

    private StackPanel CreateMainView(IWorkbenchService workbench)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(10)
            .Margin(18);

        RenderMainView(panel, workbench);
        return panel;
    }

    private void RenderMainView(StackPanel panel, IWorkbenchService workbench)
    {
        panel.Clear();
        var searchBox = new TextBox().Placeholder("Search by name, id, or publisher");

        panel.Children(
            new DockPanel().Children(
                new Button()
                    .DockRight()
                    .Content("Refresh")
                    .OnClick(() => RenderMainView(panel, workbench)),
                new Button()
                    .DockRight()
                    .Content("Install from File")
                    .OnClick(async () => await ChooseInstallPackageAsync(workbench)),
                new TextBlock().Text("Plugins").FontSize(22).SemiBold()),
            new DockPanel().Children(
                new Button()
                    .DockRight()
                    .Content("Search")
                    .OnClick(() =>
                    {
                        _searchText = searchBox.Text ?? string.Empty;
                        RenderMainView(panel, workbench);
                    }),
                searchBox),
            FilterButtons(panel, workbench));

        var entries = Entries();
        if (entries.Count == 0)
        {
            panel.Children(EmptyState());
            return;
        }

        foreach (var entry in entries)
        {
            panel.Children(PluginRow(entry, workbench));
        }
    }

    private StackPanel FilterButtons(StackPanel panel, IWorkbenchService workbench)
    {
        var filters = new[]
        {
            MewooPluginManagerCatalogFilter.All,
            MewooPluginManagerCatalogFilter.Enabled,
            MewooPluginManagerCatalogFilter.Disabled,
            MewooPluginManagerCatalogFilter.Failed,
            MewooPluginManagerCatalogFilter.Incompatible,
            MewooPluginManagerCatalogFilter.Broken,
        };

        var filterPanel = new StackPanel { Orientation = Orientation.Horizontal }.Spacing(6);
        foreach (var filter in filters)
        {
            filterPanel.Children(new Button()
                .Content(filter == _filter ? $"[{FilterLabel(filter)}]" : FilterLabel(filter))
                .OnClick(() =>
                {
                    _filter = filter;
                    RenderMainView(panel, workbench);
                }));
        }

        return filterPanel;
    }

    private Border PluginRow(MewooPluginManagerCatalogEntry entry, IWorkbenchService workbench)
    {
        var row = new Border()
            .Padding(10, 8)
            .Child(new StackPanel { Orientation = Orientation.Vertical }
                .Spacing(3)
                .Children(
                    new DockPanel().Children(
                        new TextBlock().DockRight().Text(entry.StateLabel).FontSize(12),
                        new TextBlock().Text(entry.DisplayName).SemiBold()),
                    new TextBlock().Text(entry.PluginId ?? "Unknown plugin id").FontSize(12),
                    new TextBlock().Text($"Version: {entry.Version ?? "Unknown"}").FontSize(12),
                    new TextBlock().Text($"Publisher: {FormatPublisher(entry)}").FontSize(12),
                    new TextBlock().Text($"Permissions: {entry.PermissionSummary}").FontSize(12),
                    new TextBlock().Text(entry.Message).FontSize(12)));

        row.MouseDown += e =>
        {
            if (e.Button != MouseButton.Left)
            {
                return;
            }

            _selectedEntryKey = EntryKey(entry);
            _ = workbench.OpenMainViewAsync("pluginManager.details").AsTask();
            e.Handled = true;
        };

        return row;
    }

    private Border SidebarEntry(MewooPluginManagerCatalogEntry entry, IWorkbenchService workbench)
    {
        var row = new Border()
            .Padding(6, 3)
            .ToolTip(entry.StateLabel)
            .Child(new DockPanel().Children(
                new TextBlock().DockRight().Text(entry.StateLabel).FontSize(11),
                new TextBlock().Text(entry.DisplayName)));

        row.MouseDown += e =>
        {
            if (e.Button != MouseButton.Left)
            {
                return;
            }

            _selectedEntryKey = EntryKey(entry);
            _ = workbench.OpenMainViewAsync("pluginManager.home").AsTask();
            e.Handled = true;
        };

        return row;
    }

    private Border EmptyState()
    {
        return new Border()
            .Padding(10, 8)
            .Child(new StackPanel { Orientation = Orientation.Vertical }
                .Spacing(4)
                .Children(
                    new TextBlock().Text("No local plugins match the current view.").SemiBold(),
                    new TextBlock()
                        .Text($"Use Install from File to add a local {MewooPluginPackageFormat.Extension} package.")
                        .FontSize(12)));
    }

    private IReadOnlyList<MewooPluginManagerCatalogEntry> Entries() =>
        _catalog.CreateEntries(
            _pluginRoot,
            _runtimePlugins.PluginStatuses,
            _runtimePlugins.DiscoveryIssues,
            new MewooPluginManagerCatalogQuery(_searchText, _filter));

    private StackPanel CreateInstallPreviewView(IWorkbenchService workbench)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(10)
            .Margin(18);

        RenderInstallPreview(panel, workbench);
        return panel;
    }

    private void RenderInstallPreview(StackPanel panel, IWorkbenchService workbench)
    {
        panel.Clear();
        panel.Children(new DockPanel().Children(
            new Button()
                .DockRight()
                .Content("Back")
                .OnClick(async () => await workbench.OpenMainViewAsync("pluginManager.home")),
            new TextBlock().Text("Install Plugin").FontSize(22).SemiBold()));

        if (_installPreview is null)
        {
            panel.Children(new TextBlock().Text("Choose a local plugin package to preview installation.").FontSize(12));
            panel.Children(new Button()
                .Content("Choose Plugin Package")
                .OnClick(async () => await ChooseInstallPackageAsync(workbench)));
            return;
        }

        panel.Children(
            new TextBlock().Text(_installPreview.DisplayName ?? "Plugin package").SemiBold(),
            new TextBlock().Text($"Id: {_installPreview.PluginId ?? "Unknown"}").FontSize(12),
            new TextBlock().Text($"Version: {_installPreview.Version ?? "Unknown"}").FontSize(12),
            new TextBlock().Text($"Publisher: {FormatPublisher(_installPreview)}").FontSize(12),
            new TextBlock().Text($"Trust: {_installPreview.TrustLabel}").FontSize(12),
            new TextBlock().Text($"Permissions: {_installPreview.PermissionSummary}").FontSize(12),
            new TextBlock().Text(_installPreview.TrustWarning).FontSize(12));

        if (!_installPreview.Success)
        {
            panel.Children(new TextBlock()
                .Text($"Package cannot be installed: {_installPreview.Issue?.ShortMessage ?? "Unknown error"}")
                .FontSize(12));
            panel.Children(new Button()
                .Content("Choose Another Package")
                .OnClick(async () => await ChooseInstallPackageAsync(workbench)));
            return;
        }

        panel.Children(new StackPanel { Orientation = Orientation.Horizontal }
            .Spacing(6)
            .Children(
                ActionButton("Confirm Install", async () => await InstallPreviewedPackageAsync(panel, workbench)),
                new Button()
                    .Content("Choose Another Package")
                    .OnClick(async () => await ChooseInstallPackageAsync(workbench))));
    }

    private StackPanel CreateUpdatePreviewView(IWorkbenchService workbench)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(10)
            .Margin(18);

        RenderUpdatePreview(panel, workbench);
        return panel;
    }

    private void RenderUpdatePreview(StackPanel panel, IWorkbenchService workbench)
    {
        panel.Clear();
        panel.Children(new DockPanel().Children(
            new Button()
                .DockRight()
                .Content("Back")
                .OnClick(async () => await workbench.OpenMainViewAsync("pluginManager.details")),
            new TextBlock().Text("Update Plugin").FontSize(22).SemiBold()));

        if (_updatePreview is null)
        {
            panel.Children(new TextBlock().Text("Choose a local plugin package to preview update.").FontSize(12));
            var selected = SelectedEntry();
            if (selected is not null)
            {
                panel.Children(new Button()
                    .Content("Choose Plugin Package")
                    .OnClick(async () => await ChooseUpdatePackageAsync(selected, workbench)));
            }

            return;
        }

        panel.Children(
            new TextBlock().Text(_updatePreview.DisplayName ?? "Plugin package").SemiBold(),
            new TextBlock().Text($"Installed Id: {_updatePreview.InstalledPluginId}").FontSize(12),
            new TextBlock().Text($"Package Id: {_updatePreview.PackagePluginId ?? "Unknown"}").FontSize(12),
            new TextBlock().Text($"Current Version: {_updatePreview.CurrentVersion ?? "Unknown"}").FontSize(12),
            new TextBlock().Text($"Package Version: {_updatePreview.PackageVersion ?? "Unknown"}").FontSize(12),
            new TextBlock().Text($"Version: {_updatePreview.VersionComparisonLabel}").FontSize(12),
            new TextBlock().Text($"Publisher: {FormatPublisher(_updatePreview)}").FontSize(12),
            new TextBlock().Text($"Trust: {_updatePreview.TrustLabel}").FontSize(12),
            new TextBlock().Text($"Permissions: {_updatePreview.PermissionSummary}").FontSize(12),
            new TextBlock().Text(_updatePreview.TrustWarning).FontSize(12));

        if (!_updatePreview.Success)
        {
            panel.Children(new TextBlock()
                .Text($"Update cannot continue: {_updatePreview.Message}")
                .FontSize(12));
            var selected = SelectedEntry();
            if (selected is not null)
            {
                panel.Children(new Button()
                    .Content("Choose Another Package")
                    .OnClick(async () => await ChooseUpdatePackageAsync(selected, workbench)));
            }

            return;
        }

        panel.Children(new StackPanel { Orientation = Orientation.Horizontal }
            .Spacing(6)
            .Children(
                ActionButton("Confirm Update", async () => await UpdatePreviewedPackageAsync(panel, workbench)),
                new Button()
                    .Content("Choose Another Package")
                    .OnClick(async () =>
                    {
                        var selected = SelectedEntry();
                        if (selected is not null)
                        {
                            await ChooseUpdatePackageAsync(selected, workbench);
                        }
                    })));
    }

    private StackPanel CreateDetailsView(IWorkbenchService workbench)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(10)
            .Margin(18);

        RenderDetailsView(panel, workbench);
        return panel;
    }

    private void RenderDetailsView(StackPanel panel, IWorkbenchService workbench)
    {
        panel.Clear();
        var entry = SelectedEntry();
        panel.Children(new DockPanel().Children(
            new Button()
                .DockRight()
                .Content("Back")
                .OnClick(async () => await workbench.OpenMainViewAsync("pluginManager.home")),
            new TextBlock().Text("Plugin Details").FontSize(22).SemiBold()));

        if (entry is null)
        {
            panel.Children(new TextBlock().Text("Select a plugin from Plugin Manager to view details.").FontSize(12));
            return;
        }

        panel.Children(
            new TextBlock().Text(entry.DisplayName).SemiBold(),
            new TextBlock().Text($"Id: {entry.PluginId ?? "Unknown"}").FontSize(12),
            new TextBlock().Text($"Version: {entry.Version ?? "Unknown"}").FontSize(12),
            new TextBlock().Text($"Publisher: {FormatPublisher(entry)}").FontSize(12),
            new TextBlock().Text($"Status: {entry.StateLabel}").FontSize(12),
            new TextBlock().Text($"Trust: {entry.TrustLabel}").FontSize(12),
            new TextBlock().Text($"Permissions: {entry.PermissionSummary}").FontSize(12),
            new TextBlock().Text($"Message: {entry.Message}").FontSize(12),
            DetailActions(entry, panel, workbench),
            RecentOperationsBlock());
    }

    private StackPanel DetailActions(
        MewooPluginManagerCatalogEntry entry,
        StackPanel panel,
        IWorkbenchService workbench)
    {
        var actions = new StackPanel { Orientation = Orientation.Horizontal }.Spacing(6);
        if (entry.PluginId is not null && entry.State == MewooPluginManagerCatalogState.Disabled)
        {
            actions.Children(ActionButton("Enable", async () =>
            {
                if (_runtimePlugins.SetPluginDisabled(entry.PluginId, disabled: false))
                {
                    await _runtimePlugins.ReloadPluginAsync(
                        entry.PluginId,
                        _pluginRoot,
                        _pluginHost,
                        plugin => new PluginManagerPluginContext(plugin.Id, _services, workbench));
                    AddOperation($"Enabled {entry.DisplayName}");
                }

                RenderDetailsView(panel, workbench);
            }));
        }
        else if (entry.PluginId is not null && entry.State != MewooPluginManagerCatalogState.Broken)
        {
            actions.Children(ActionButton("Disable", async () =>
            {
                if (_runtimePlugins.SetPluginDisabled(entry.PluginId, disabled: true))
                {
                    await _runtimePlugins.UnloadPluginAsync(entry.PluginId, _pluginHost);
                    AddOperation($"Disabled {entry.DisplayName}");
                }

                RenderDetailsView(panel, workbench);
            }));
        }

        if (entry.PluginId is not null && entry.State != MewooPluginManagerCatalogState.Broken)
        {
            actions.Children(ActionButton("Update from File", async () =>
            {
                await ChooseUpdatePackageAsync(entry, workbench);
            }));
        }

        if (entry.State == MewooPluginManagerCatalogState.Broken)
        {
            actions.Children(ActionButton("Remove Broken Install", async () =>
            {
                var result = await _packageOperations.RemoveBrokenInstallAsync(
                    entry,
                    _pluginRoot);
                AddOperation(result.Success
                    ? $"Removed broken install {entry.DisplayName}"
                    : $"Remove broken install failed: {result.Issue?.ShortMessage ?? "Unknown error"}");
                _selectedEntryKey = null;
                RenderDetailsView(panel, workbench);
            }));
        }
        else if (entry.PluginId is not null)
        {
            actions.Children(ActionButton("Uninstall", async () =>
            {
                var result = await _packageOperations.UninstallAsync(
                    entry.PluginId,
                    _pluginRoot,
                    _runtimePlugins,
                    _pluginHost);
                AddOperation(result.Success
                    ? $"Uninstalled {entry.DisplayName}"
                    : $"Uninstall failed: {result.Issue?.ShortMessage ?? "Unknown error"}");
                _selectedEntryKey = null;
                RenderDetailsView(panel, workbench);
            }));
        }

        actions.Children(ActionButton("Open Diagnostics", async () =>
            await workbench.OpenMainViewAsync("runtimeDiagnostics.home")));

        return actions;
    }

    private StackPanel RecentOperationsBlock()
    {
        var block = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(4)
            .Children(new TextBlock().Text("Recent Operations").SemiBold().Margin(0, 10, 0, 0));
        if (_operationResults.Count == 0)
        {
            block.Children(new TextBlock().Text("No plugin operations have run in this session.").FontSize(12));
            return block;
        }

        foreach (var operation in _operationResults.Take(8))
        {
            block.Children(new TextBlock().Text(operation).FontSize(12));
        }

        return block;
    }

    private MewooPluginManagerCatalogEntry? SelectedEntry()
    {
        if (string.IsNullOrWhiteSpace(_selectedEntryKey))
        {
            return null;
        }

        return _catalog.CreateEntries(
                _pluginRoot,
                _runtimePlugins.PluginStatuses,
                _runtimePlugins.DiscoveryIssues)
            .FirstOrDefault(entry => string.Equals(EntryKey(entry), _selectedEntryKey, StringComparison.Ordinal));
    }

    private async Task ChooseInstallPackageAsync(IWorkbenchService workbench)
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

        _installPreview = _installPreviewer.Preview(packagePath);
        await workbench.OpenMainViewAsync("pluginManager.installPreview");
    }

    private async Task ChooseUpdatePackageAsync(
        MewooPluginManagerCatalogEntry entry,
        IWorkbenchService workbench)
    {
        if (string.IsNullOrWhiteSpace(entry.PluginId))
        {
            AddOperation($"Update failed: {entry.DisplayName} does not have a readable plugin id.");
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

        _selectedEntryKey = EntryKey(entry);
        _updatePreview = _updatePreviewer.Preview(packagePath, entry);
        await workbench.OpenMainViewAsync("pluginManager.updatePreview");
    }

    private async Task InstallPreviewedPackageAsync(StackPanel panel, IWorkbenchService workbench)
    {
        if (_installPreview is not { Success: true })
        {
            AddOperation("Install failed: no valid package preview.");
            RenderInstallPreview(panel, workbench);
            return;
        }

        var result = _installer.Install(_installPreview.PackagePath, _pluginRoot);
        var summary = MewooPluginInstallFlowDisplay.CreateResultSummary(result);
        AddOperation(summary.Success ? summary.Message : $"Install failed: {summary.Message}");
        if (summary.Success && summary.PluginId is not null)
        {
            await _runtimePlugins.ReloadPluginAsync(
                summary.PluginId,
                _pluginRoot,
                _pluginHost,
                plugin => new PluginManagerPluginContext(plugin.Id, _services, workbench));
            _selectedEntryKey = summary.PluginId;
            _installPreview = null;
            await workbench.OpenMainViewAsync("pluginManager.details");
            return;
        }

        RenderInstallPreview(panel, workbench);
    }

    private async Task UpdatePreviewedPackageAsync(StackPanel panel, IWorkbenchService workbench)
    {
        if (_updatePreview is not { Success: true })
        {
            AddOperation("Update failed: no valid package preview.");
            RenderUpdatePreview(panel, workbench);
            return;
        }

        var result = await _packageOperations.UpdateAsync(
            _updatePreview.PackagePath,
            _pluginRoot,
            _runtimePlugins,
            _pluginHost,
            plugin => new PluginManagerPluginContext(plugin.Id, _services, workbench));
        var summary = MewooPluginUpdateFlowDisplay.CreateResultSummary(result);
        AddOperation(summary.Success ? summary.Message : $"Update failed: {summary.Message}");
        if (summary.Success && summary.PluginId is not null)
        {
            _selectedEntryKey = summary.PluginId;
            _updatePreview = null;
            await workbench.OpenMainViewAsync("pluginManager.details");
            return;
        }

        RenderUpdatePreview(panel, workbench);
    }

    private void AddOperation(string message)
    {
        _operationResults.Insert(0, $"[{DateTimeOffset.Now:HH:mm:ss}] {message}");
        if (_operationResults.Count > 25)
        {
            _operationResults.RemoveRange(25, _operationResults.Count - 25);
        }
    }

    private static Button ActionButton(string text, Func<Task> action)
    {
        return new Button()
            .Content(text)
            .OnClick(async () => await action());
    }

    private static string EntryKey(MewooPluginManagerCatalogEntry entry) =>
        entry.PluginId ?? entry.PluginDirectory;

    private static string FilterLabel(MewooPluginManagerCatalogFilter filter) =>
        filter switch
        {
            MewooPluginManagerCatalogFilter.All => "All",
            MewooPluginManagerCatalogFilter.Enabled => "Enabled",
            MewooPluginManagerCatalogFilter.Disabled => "Disabled",
            MewooPluginManagerCatalogFilter.Failed => "Failed",
            MewooPluginManagerCatalogFilter.Incompatible => "Incompatible",
            MewooPluginManagerCatalogFilter.Broken => "Broken",
            _ => filter.ToString(),
        };

    private static string FormatPublisher(MewooPluginManagerCatalogEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.PublisherDisplayName) && !string.IsNullOrWhiteSpace(entry.Publisher))
        {
            return $"{entry.PublisherDisplayName} ({entry.Publisher})";
        }

        return entry.PublisherDisplayName ?? entry.Publisher ?? "Not declared";
    }

    private static string FormatPublisher(MewooPluginInstallPreview preview)
    {
        if (!string.IsNullOrWhiteSpace(preview.PublisherDisplayName) && !string.IsNullOrWhiteSpace(preview.Publisher))
        {
            return $"{preview.PublisherDisplayName} ({preview.Publisher})";
        }

        return preview.PublisherDisplayName ?? preview.Publisher ?? "Not declared";
    }

    private static string FormatPublisher(MewooPluginUpdatePreview preview)
    {
        if (!string.IsNullOrWhiteSpace(preview.PublisherDisplayName) && !string.IsNullOrWhiteSpace(preview.Publisher))
        {
            return $"{preview.PublisherDisplayName} ({preview.Publisher})";
        }

        return preview.PublisherDisplayName ?? preview.Publisher ?? "Not declared";
    }

    private sealed record PluginManagerPluginContext(
        string PluginId,
        IServiceProvider Services,
        IWorkbenchService Workbench) : IMewooPluginContext;
}
