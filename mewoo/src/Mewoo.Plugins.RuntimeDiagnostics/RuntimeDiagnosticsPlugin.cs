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
    private readonly IMewooLogger _logger;

    public RuntimeDiagnosticsPlugin(MewooRuntimePluginManager runtimePlugins, IMewooLogger logger)
    {
        _runtimePlugins = runtimePlugins;
        _logger = logger;
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
            .Create(_ => new MewooView("runtimeDiagnostics.home", CreateMainView()));

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

    private StackPanel CreateMainView()
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(10)
            .Margin(18)
            .Children(new TextBlock().Text("Runtime Diagnostics").FontSize(22).SemiBold());

        if (_runtimePlugins.PluginStatuses.Count == 0)
        {
            panel.Children(new TextBlock().Text("No runtime plugins are discovered.").FontSize(12));
        }
        else
        {
            foreach (var status in _runtimePlugins.PluginStatuses)
            {
                panel.Children(RuntimePluginBlock(status));
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

        return panel;
    }

    private static Border RuntimePluginBlock(MewooRuntimePluginStatus status)
    {
        var descriptor = status.Descriptor;
        var manifest = descriptor.Manifest;
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
                    new TextBlock().Text(status.Message ?? string.Empty).FontSize(12)));
    }
}
