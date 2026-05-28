using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Mewoo.Abstractions;
using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Logging;
using Mewoo.Abstractions.Plugins;
using Mewoo.Abstractions.Views;
using Mewoo.Core.Plugins;

namespace Mewoo.Plugins.RuntimeDiagnostics;

public sealed class RuntimeDiagnosticsPlugin : IMewooPlugin
{
    private readonly MewooRuntimePluginManager _runtimePlugins;
    private readonly MewooPluginHost _pluginHost;
    private readonly MewooPluginPackageOperations _packageOperations = new();
    private readonly List<MewooPluginCatalogOperation> _operationResults = [];
    private readonly IMewooLogger _logger;
    private readonly string _pluginRoot;
    private readonly IServiceProvider _services;

    public RuntimeDiagnosticsPlugin(
        MewooRuntimePluginManager runtimePlugins,
        MewooPluginHost pluginHost,
        IMewooLogger logger,
        string pluginRoot,
        IServiceProvider services)
    {
        _runtimePlugins = runtimePlugins;
        _pluginHost = pluginHost;
        _logger = logger;
        _pluginRoot = pluginRoot;
        _services = services;
    }

    public string Id => "runtimeDiagnostics";

    public string DisplayName => "Runtime Diagnostics";

    public void Register(IMewooContributionRegistry registry)
    {
        registry.Activity("runtimeDiagnostics.activity")
            .Title("Runtime")
            .Icon("R")
            .ViewContainer("runtimeDiagnostics.views")
            .Order(90);

        registry.ViewContainer("runtimeDiagnostics.views")
            .Title("Runtime")
            .AddView("runtimeDiagnostics.summary", view => view
                .Title("Plugins")
                .Create(ctx => new MewooView("runtimeDiagnostics.summary", CreateSidebar(ctx.Workbench))));

        registry.MainView("runtimeDiagnostics.home")
            .Title("Runtime Diagnostics")
            .CanOpenMultiple(false)
            .Create(ctx => new MewooView("runtimeDiagnostics.home", CreateMainView(ctx.Workbench)));

        registry.Command("runtimeDiagnostics.open")
            .Title("Open Runtime Diagnostics")
            .Category("Runtime")
            .Execute(async (ctx, cancellationToken) =>
                await ctx.Workbench.OpenMainViewAsync("runtimeDiagnostics.home", cancellationToken));
    }

    private StackPanel CreateSidebar(IWorkbenchService workbench)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(8)
            .Margin(12)
            .Children(
                new TextBlock().Text("Runtime Plugins").SemiBold(),
                new TextBlock().Text($"{_runtimePlugins.LoadedPlugins.Count} loaded").FontSize(12),
                new Button()
                    .Content("Open Diagnostics")
                    .OnClick(async () => await workbench.OpenMainViewAsync("runtimeDiagnostics.home")));

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
        panel.Children(
            new DockPanel().Children(
                new Button()
                    .DockRight()
                    .Content("Refresh")
                    .OnClick(() => RenderMainView(panel, workbench)),
                new Button()
                    .DockRight()
                    .Content("Open Logs")
                    .OnClick(workbench.OpenLogsPanel),
                new TextBlock().Text("Runtime Diagnostics").FontSize(22).SemiBold()));

        if (_runtimePlugins.PluginStatuses.Count == 0 && _runtimePlugins.DiscoveryIssues.Count == 0)
        {
            panel.Children(new TextBlock()
                .Text($"No local plugin packages are installed. Add an installed plugin folder under '{_pluginRoot}', or place an update package named '<pluginId>{MewooPluginPackageFormat.Extension}' in that directory for an installed plugin.")
                .FontSize(12));
        }
        else
        {
            var catalogEntries = MewooPluginCatalogDisplay.CreateEntries(
                _runtimePlugins.PluginStatuses,
                _runtimePlugins.DiscoveryIssues);
            foreach (var entry in catalogEntries.Where(entry => entry.State == MewooPluginCatalogEntryState.Discovered))
            {
                panel.Children(RuntimeDiscoveryIssueBlock(entry));
            }

            foreach (var status in _runtimePlugins.PluginStatuses)
            {
                var entry = catalogEntries.First(item =>
                    string.Equals(item.PluginId, status.Descriptor.Manifest.Id, StringComparison.Ordinal));
                panel.Children(RuntimePluginBlock(status, entry, panel, workbench));
            }
        }

        panel.Children(new TextBlock().Text("Recent Package Operations").SemiBold().Margin(0, 12, 0, 0));
        if (_operationResults.Count == 0)
        {
            panel.Children(new TextBlock().Text("No install, update, or uninstall operations have run in this session.").FontSize(12));
        }
        else
        {
            foreach (var operation in _operationResults.Take(10))
            {
                panel.Children(new TextBlock()
                    .Text($"[{operation.Timestamp:HH:mm:ss}] {operation.Operation} {(operation.Success ? "OK" : "Failed")} {operation.PluginId ?? "unknown"}: {operation.Message}")
                    .FontSize(12));
            }
        }

        var runtimeLogs = _logger.Entries
            .Where(entry => entry.Source.StartsWith("RuntimePlugin", StringComparison.Ordinal))
            .TakeLast(25)
            .ToArray();

        panel.Children(new TextBlock().Text("Recent Runtime Logs").SemiBold().Margin(0, 12, 0, 0));
        if (runtimeLogs.Length == 0)
        {
            panel.Children(new TextBlock().Text("No runtime plugin logs yet.").FontSize(12));
        }
        else
        {
            foreach (var entry in runtimeLogs)
            {
                panel.Children(new TextBlock()
                    .Text($"[{entry.Timestamp:HH:mm:ss}] {entry.Level} {entry.Source}: {entry.Message}")
                    .FontSize(12));
            }
        }
    }

    private Border RuntimePluginBlock(
        MewooRuntimePluginStatus status,
        MewooPluginCatalogEntry entry,
        StackPanel panel,
        IWorkbenchService workbench)
    {
        var descriptor = status.Descriptor;
        var manifest = descriptor.Manifest;
        var actions = new StackPanel { Orientation = Orientation.Horizontal }.Spacing(6);
        actions.Children(ActionButton("Reload", async () =>
        {
            await _runtimePlugins.ReloadPluginAsync(
                manifest.Id,
                _pluginRoot,
                _pluginHost,
                plugin => new RuntimePluginContext(plugin.Id, _services, workbench));
            RenderMainView(panel, workbench);
        }));

        if (status.State is MewooRuntimePluginState.Loaded or MewooRuntimePluginState.Registered)
        {
            actions.Children(ActionButton("Unload", async () =>
            {
                await _runtimePlugins.UnloadPluginAsync(manifest.Id, _pluginHost);
                RenderMainView(panel, workbench);
            }));
        }

        if (status.State == MewooRuntimePluginState.Disabled)
        {
            actions.Children(ActionButton("Enable", async () =>
            {
                if (_runtimePlugins.SetPluginDisabled(manifest.Id, disabled: false))
                {
                    await _runtimePlugins.ReloadPluginAsync(
                        manifest.Id,
                        _pluginRoot,
                        _pluginHost,
                        plugin => new RuntimePluginContext(plugin.Id, _services, workbench));
                }

                RenderMainView(panel, workbench);
            }));
        }
        else
        {
            actions.Children(ActionButton("Disable", async () =>
            {
                if (_runtimePlugins.SetPluginDisabled(manifest.Id, disabled: true))
                {
                    await _runtimePlugins.UnloadPluginAsync(manifest.Id, _pluginHost);
                }

                RenderMainView(panel, workbench);
            }));
        }

        actions.Children(ActionButton("Update", async () =>
        {
            var packagePath = Path.Combine(_pluginRoot, $"{manifest.Id}{MewooPluginPackageFormat.Extension}");
            var result = await _packageOperations.UpdateAsync(
                packagePath,
                _pluginRoot,
                _runtimePlugins,
                _pluginHost,
                plugin => new RuntimePluginContext(plugin.Id, _services, workbench));

            LogOperationResult("Update", result);
            RenderMainView(panel, workbench);
        }));

        actions.Children(ActionButton("Uninstall", async () =>
        {
            var result = await _packageOperations.UninstallAsync(
                manifest.Id,
                _pluginRoot,
                _runtimePlugins,
                _pluginHost);

            LogOperationResult("Uninstall", result);
            RenderMainView(panel, workbench);
        }));

        return new Border()
            .Padding(10, 8)
            .Child(new StackPanel { Orientation = Orientation.Vertical }
                .Spacing(4)
                .Children(
                    new TextBlock().Text($"{manifest.DisplayName} ({manifest.Id})").SemiBold(),
                    new TextBlock().Text($"Catalog State: {entry.StateLabel}").FontSize(12),
                    new TextBlock().Text($"Package Version: {entry.Version ?? "Unknown"}").FontSize(12),
                    new TextBlock().Text($"Publisher: {FormatPublisher(entry)}").FontSize(12),
                    new TextBlock().Text($"Trust: {entry.TrustLabel}").FontSize(12),
                    new TextBlock().Text($"Permissions: {entry.PermissionSummary}").FontSize(12),
                    new TextBlock().Text($"Manifest: {descriptor.ManifestPath}").FontSize(12),
                    new TextBlock().Text($"Assembly: {descriptor.AssemblyPath}").FontSize(12),
                    new TextBlock().Text($"State: {status.State}").FontSize(12),
                    new TextBlock().Text($"Category: {status.CategoryLabel}").FontSize(12),
                    new TextBlock().Text($"Message: {status.ShortMessage}").FontSize(12),
                    actions));
    }

    private static Border RuntimeDiscoveryIssueBlock(MewooPluginCatalogEntry entry)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(4)
            .Children(
                new TextBlock().Text(entry.DisplayName).SemiBold(),
                new TextBlock().Text($"Catalog State: {entry.StateLabel}").FontSize(12),
                new TextBlock().Text($"Trust: {entry.TrustLabel}").FontSize(12),
                new TextBlock().Text($"Permissions: {entry.PermissionSummary}").FontSize(12),
                new TextBlock().Text($"Category: {entry.CategoryLabel}").FontSize(12),
                new TextBlock().Text($"Message: {entry.Message}").FontSize(12),
                new TextBlock().Text($"Manifest: {entry.ManifestPath}").FontSize(12));

        if (!string.IsNullOrWhiteSpace(entry.AssemblyPath))
        {
            panel.Children(new TextBlock().Text($"Assembly: {entry.AssemblyPath}").FontSize(12));
        }

        return new Border()
            .Padding(10, 8)
            .Child(panel);
    }

    private static Button ActionButton(string text, Func<Task> action)
    {
        return new Button()
            .Content(text)
            .OnClick(async () => await action());
    }

    private void LogOperationResult(string operation, MewooPluginOperationResult result)
    {
        _operationResults.Insert(0, MewooPluginCatalogDisplay.CreateOperation(operation, result, DateTimeOffset.Now));
        if (_operationResults.Count > 25)
        {
            _operationResults.RemoveRange(25, _operationResults.Count - 25);
        }

        if (result.Success)
        {
            _logger.Info(
                "RuntimePlugin.PackageOperation",
                $"{operation} succeeded for plugin '{result.PluginId}' at {result.Path}.");
            return;
        }

        _logger.Error(
            "RuntimePlugin.PackageOperation",
            $"{operation} failed for plugin '{result.PluginId ?? "unknown"}': {result.Issue?.ShortMessage ?? "Unknown error"}");
    }

    private static string FormatPublisher(MewooPluginCatalogEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.PublisherDisplayName) && !string.IsNullOrWhiteSpace(entry.Publisher))
        {
            return $"{entry.PublisherDisplayName} ({entry.Publisher})";
        }

        return entry.PublisherDisplayName ?? entry.Publisher ?? "Not declared";
    }

    private sealed record RuntimePluginContext(
        string PluginId,
        IServiceProvider Services,
        IWorkbenchService Workbench) : IMewooPluginContext;
}
