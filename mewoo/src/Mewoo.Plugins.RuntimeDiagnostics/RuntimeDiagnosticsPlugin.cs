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
                .Text("No runtime plugins are discovered. Add a plugin folder with mewoo.plugin.json under the runtime plugin directory.")
                .FontSize(12));
        }
        else
        {
            foreach (var issue in _runtimePlugins.DiscoveryIssues)
            {
                panel.Children(RuntimeDiscoveryIssueBlock(issue));
            }

            foreach (var status in _runtimePlugins.PluginStatuses)
            {
                panel.Children(RuntimePluginBlock(status, panel, workbench));
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

    private Border RuntimePluginBlock(MewooRuntimePluginStatus status, StackPanel panel, IWorkbenchService workbench)
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
                    new TextBlock().Text($"Version: {manifest.Version}").FontSize(12),
                    new TextBlock().Text($"Manifest: {descriptor.ManifestPath}").FontSize(12),
                    new TextBlock().Text($"Assembly: {descriptor.AssemblyPath}").FontSize(12),
                    new TextBlock().Text($"State: {status.State}").FontSize(12),
                    new TextBlock().Text($"Category: {status.CategoryLabel}").FontSize(12),
                    new TextBlock().Text($"Message: {status.ShortMessage}").FontSize(12),
                    actions));
    }

    private static Border RuntimeDiscoveryIssueBlock(MewooRuntimePluginIssue issue)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(4)
            .Children(
                new TextBlock().Text("Runtime plugin discovery issue").SemiBold(),
                new TextBlock().Text($"Category: {issue.Category}").FontSize(12),
                new TextBlock().Text($"Message: {issue.ShortMessage}").FontSize(12),
                new TextBlock().Text($"Manifest: {issue.ManifestPath}").FontSize(12));

        if (!string.IsNullOrWhiteSpace(issue.AssemblyPath))
        {
            panel.Children(new TextBlock().Text($"Assembly: {issue.AssemblyPath}").FontSize(12));
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

    private sealed record RuntimePluginContext(
        string PluginId,
        IServiceProvider Services,
        IWorkbenchService Workbench) : IMewooPluginContext;
}
