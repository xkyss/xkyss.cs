using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Mewoo.Abstractions;
using Mewoo.Core.Plugins;

namespace Mewoo.Plugins.PluginManager;

internal sealed class PluginManagerDiagnosticViews
{
    private readonly PluginManagerSession _session;
    private readonly PluginManagerController _controller;

    public PluginManagerDiagnosticViews(
        PluginManagerSession session,
        PluginManagerController controller)
    {
        _session = session;
        _controller = controller;
    }

    public StackPanel CreateRuntimeStatusView(IWorkbenchService workbench)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(10)
            .Margin(18);

        RenderRuntimeStatus(panel, workbench);
        return panel;
    }

    public StackPanel CreateDiscoveryIssuesView(IWorkbenchService workbench)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(10)
            .Margin(18);

        RenderDiscoveryIssues(panel, workbench);
        return panel;
    }

    public StackPanel CreateOperationLogView(IWorkbenchService workbench)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(10)
            .Margin(18);

        RenderOperationLog(panel, workbench);
        return panel;
    }

    private void RenderRuntimeStatus(StackPanel panel, IWorkbenchService workbench)
    {
        panel.Clear();
        panel.Children(
            new DockPanel().Children(
                new Button()
                    .DockRight()
                    .Content("Refresh")
                    .OnClick(() => RenderRuntimeStatus(panel, workbench)),
                new Button()
                    .DockRight()
                    .Content("Open Logs")
                    .OnClick(workbench.OpenLogsPanel),
                new TextBlock().Text("Runtime Status").FontSize(22).SemiBold()));

        if (_controller.RuntimeStatuses.Count == 0)
        {
            panel.Children(new TextBlock()
                .Text("No runtime plugin statuses are available.")
                .FontSize(12));
            return;
        }

        var catalogEntries = _controller.DiagnosticCatalogEntries();
        foreach (var status in _controller.RuntimeStatuses)
        {
            var entry = catalogEntries.First(item =>
                string.Equals(item.PluginId, status.Descriptor.Manifest.Id, StringComparison.Ordinal));
            panel.Children(RuntimePluginBlock(status, entry, panel, workbench));
        }
    }

    private void RenderDiscoveryIssues(StackPanel panel, IWorkbenchService workbench)
    {
        panel.Clear();
        panel.Children(
            new DockPanel().Children(
                new Button()
                    .DockRight()
                    .Content("Refresh")
                    .OnClick(() => RenderDiscoveryIssues(panel, workbench)),
                new TextBlock().Text("Discovery Issues").FontSize(22).SemiBold()));

        var catalogEntries = _controller.DiagnosticCatalogEntries()
            .Where(entry => entry.State == MewooPluginCatalogEntryState.Discovered)
            .ToArray();
        if (catalogEntries.Length == 0)
        {
            panel.Children(new TextBlock().Text("No runtime plugin discovery issues.").FontSize(12));
            return;
        }

        foreach (var entry in catalogEntries)
        {
            panel.Children(RuntimeDiscoveryIssueBlock(entry));
        }
    }

    private void RenderOperationLog(StackPanel panel, IWorkbenchService workbench)
    {
        panel.Clear();
        panel.Children(
            new DockPanel().Children(
                new Button()
                    .DockRight()
                    .Content("Open Logs")
                    .OnClick(workbench.OpenLogsPanel),
                new TextBlock().Text("Operation Log").FontSize(22).SemiBold()));

        panel.Children(new TextBlock().Text("Recent Plugin Manager Operations").SemiBold());
        if (_session.OperationResults.Count == 0)
        {
            panel.Children(new TextBlock().Text("No plugin operations have run in this session.").FontSize(12));
        }
        else
        {
            foreach (var operation in _session.OperationResults.Take(10))
            {
                panel.Children(new TextBlock().Text(operation).FontSize(12));
            }
        }

        var runtimeLogs = _controller.RuntimeLogs();
        panel.Children(new TextBlock().Text("Recent Runtime Logs").SemiBold().Margin(0, 12, 0, 0));
        if (runtimeLogs.Count == 0)
        {
            panel.Children(new TextBlock().Text("No runtime plugin logs yet.").FontSize(12));
            return;
        }

        foreach (var entry in runtimeLogs)
        {
            panel.Children(new TextBlock()
                .Text($"[{entry.Timestamp:HH:mm:ss}] {entry.Level} {entry.Source}: {entry.Message}")
                .FontSize(12));
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
            await _controller.ReloadRuntimePluginAsync(status, workbench);
            RenderRuntimeStatus(panel, workbench);
        }));

        if (status.State is MewooRuntimePluginState.Loaded or MewooRuntimePluginState.Registered)
        {
            actions.Children(ActionButton("Unload", async () =>
            {
                await _controller.UnloadRuntimePluginAsync(status);
                RenderRuntimeStatus(panel, workbench);
            }));
        }

        return new Border()
            .Padding(10, 8)
            .Child(new StackPanel { Orientation = Orientation.Vertical }
                .Spacing(4)
                .Children(
                    new TextBlock().Text($"{manifest.DisplayName} ({manifest.Id})").SemiBold(),
                    new TextBlock().Text($"Catalog State: {entry.StateLabel}").FontSize(12),
                    new TextBlock().Text($"Package Version: {entry.Version ?? "Unknown"}").FontSize(12),
                    new TextBlock().Text($"Publisher: {PluginManagerFormatting.FormatCatalogPublisher(entry)}").FontSize(12),
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
}
