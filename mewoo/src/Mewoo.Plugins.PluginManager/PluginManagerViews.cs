using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Mewoo.Abstractions;
using Mewoo.Core.Plugins;
using Mewoo.Controls.Sidebar;

namespace Mewoo.Plugins.PluginManager;

internal sealed class PluginManagerViews
{
    private readonly PluginManagerSession _session;
    private readonly PluginManagerController _controller;

    public PluginManagerViews(
        PluginManagerSession session,
        PluginManagerController controller)
    {
        _session = session;
        _controller = controller;
        Diagnostics = new PluginManagerDiagnosticViews(session, controller);
    }

    public PluginManagerDiagnosticViews Diagnostics { get; }

    public StackPanel CreateSidebar(IWorkbenchService workbench)
    {
        var navigation = SidebarNavigation.Create(
                workbench,
                new SidebarNavigationOptions
                {
                    InitialExpandedNodeIds = new HashSet<string>(["manage"], StringComparer.Ordinal),
                })
            .Node("manage", "Manage", manage =>
            {
                AddCategoryAction(manage, PluginManagerCategory.All, workbench);
                AddCategoryAction(manage, PluginManagerCategory.Enabled, workbench);
                AddCategoryAction(manage, PluginManagerCategory.Disabled, workbench);
                AddCategoryAction(manage, PluginManagerCategory.NeedsAttention, workbench);
            })
            .Node("advanced", AdvancedLabel(), advanced =>
            {
                advanced.MainView("advanced.runtimeStatus", RuntimeStatusLabel(), "pluginManager.runtimeStatus");
                advanced.MainView("advanced.discoveryIssues", DiscoveryIssuesLabel(), "pluginManager.discoveryIssues");
                advanced.MainView("advanced.operationLog", "Operation Log", "pluginManager.operationLog");
            })
            .Build()
            .Margin(4, 0, 4, 0);

        return SidebarLayout.Create()
            .Header(SidebarHeader.Create("Plugins")
                .Action("+", "Install from file", async () => await _controller.ChooseInstallPackageAsync(workbench))
                .More(menu => menu.Item("Open Plugin Manager", async () =>
                    await workbench.OpenMainViewAsync("pluginManager.home"))))
            .Body(navigation)
            .Build();
    }

    public StackPanel CreateMainView(IWorkbenchService workbench)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(10)
            .Margin(18);

        RenderMainView(panel, workbench);
        return panel;
    }

    public StackPanel CreateInstallPreviewView(IWorkbenchService workbench)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(10)
            .Margin(18);

        RenderInstallPreview(panel, workbench);
        return panel;
    }

    public StackPanel CreateUpdatePreviewView(IWorkbenchService workbench)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(10)
            .Margin(18);

        RenderUpdatePreview(panel, workbench);
        return panel;
    }

    public StackPanel CreateDetailsView(IWorkbenchService workbench)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(10)
            .Margin(18);

        RenderDetailsView(panel, workbench);
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
                    .OnClick(async () => await _controller.ChooseInstallPackageAsync(workbench)),
                new TextBlock().Text($"Plugins: {PluginManagerFormatting.CategoryLabel(_session.Category)}").FontSize(22).SemiBold()),
            new DockPanel().Children(
                new Button()
                    .DockRight()
                    .Content("Search")
                    .OnClick(() =>
                    {
                        _session.SearchText = searchBox.Text ?? string.Empty;
                        RenderMainView(panel, workbench);
                    }),
                searchBox));

        var entries = _controller.Entries();
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
                    new TextBlock().Text($"Publisher: {PluginManagerFormatting.FormatPublisher(entry)}").FontSize(12),
                    new TextBlock().Text($"Permissions: {entry.PermissionSummary}").FontSize(12),
                    new TextBlock().Text(entry.Message).FontSize(12)));

        row.MouseDown += e =>
        {
            if (e.Button != MouseButton.Left)
            {
                return;
            }

            _session.SelectedEntryKey = PluginManagerFormatting.EntryKey(entry);
            _ = workbench.OpenMainViewAsync("pluginManager.details").AsTask();
            e.Handled = true;
        };

        return row;
    }

    private static Border EmptyState()
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

    private void RenderInstallPreview(StackPanel panel, IWorkbenchService workbench)
    {
        panel.Clear();
        panel.Children(new DockPanel().Children(
            new Button()
                .DockRight()
                .Content("Back")
                .OnClick(async () => await workbench.OpenMainViewAsync("pluginManager.home")),
            new TextBlock().Text("Install Plugin").FontSize(22).SemiBold()));

        if (_session.InstallPreview is null)
        {
            panel.Children(new TextBlock().Text("Choose a local plugin package to preview installation.").FontSize(12));
            panel.Children(new Button()
                .Content("Choose Plugin Package")
                .OnClick(async () => await _controller.ChooseInstallPackageAsync(workbench)));
            return;
        }

        panel.Children(
            new TextBlock().Text(_session.InstallPreview.DisplayName ?? "Plugin package").SemiBold(),
            new TextBlock().Text($"Id: {_session.InstallPreview.PluginId ?? "Unknown"}").FontSize(12),
            new TextBlock().Text($"Version: {_session.InstallPreview.Version ?? "Unknown"}").FontSize(12),
            new TextBlock().Text($"Publisher: {PluginManagerFormatting.FormatPublisher(_session.InstallPreview)}").FontSize(12),
            new TextBlock().Text($"Trust: {_session.InstallPreview.TrustLabel}").FontSize(12),
            new TextBlock().Text($"Permissions: {_session.InstallPreview.PermissionSummary}").FontSize(12),
            new TextBlock().Text(_session.InstallPreview.TrustWarning).FontSize(12));

        if (!_session.InstallPreview.Success)
        {
            panel.Children(new TextBlock()
                .Text($"Package cannot be installed: {_session.InstallPreview.Issue?.ShortMessage ?? "Unknown error"}")
                .FontSize(12));
            panel.Children(new Button()
                .Content("Choose Another Package")
                .OnClick(async () => await _controller.ChooseInstallPackageAsync(workbench)));
            return;
        }

        panel.Children(new StackPanel { Orientation = Orientation.Horizontal }
            .Spacing(6)
            .Children(
                ActionButton("Confirm Install", async () =>
                {
                    if (!await _controller.InstallPreviewedPackageAsync(workbench))
                    {
                        RenderInstallPreview(panel, workbench);
                    }
                }),
                new Button()
                    .Content("Choose Another Package")
                    .OnClick(async () => await _controller.ChooseInstallPackageAsync(workbench))));
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

        if (_session.UpdatePreview is null)
        {
            panel.Children(new TextBlock().Text("Choose a local plugin package to preview update.").FontSize(12));
            var selected = _controller.SelectedEntry();
            if (selected is not null)
            {
                panel.Children(new Button()
                    .Content("Choose Plugin Package")
                    .OnClick(async () => await _controller.ChooseUpdatePackageAsync(selected, workbench)));
            }

            return;
        }

        panel.Children(
            new TextBlock().Text(_session.UpdatePreview.DisplayName ?? "Plugin package").SemiBold(),
            new TextBlock().Text($"Installed Id: {_session.UpdatePreview.InstalledPluginId}").FontSize(12),
            new TextBlock().Text($"Package Id: {_session.UpdatePreview.PackagePluginId ?? "Unknown"}").FontSize(12),
            new TextBlock().Text($"Current Version: {_session.UpdatePreview.CurrentVersion ?? "Unknown"}").FontSize(12),
            new TextBlock().Text($"Package Version: {_session.UpdatePreview.PackageVersion ?? "Unknown"}").FontSize(12),
            new TextBlock().Text($"Version: {_session.UpdatePreview.VersionComparisonLabel}").FontSize(12),
            new TextBlock().Text($"Publisher: {PluginManagerFormatting.FormatPublisher(_session.UpdatePreview)}").FontSize(12),
            new TextBlock().Text($"Trust: {_session.UpdatePreview.TrustLabel}").FontSize(12),
            new TextBlock().Text($"Permissions: {_session.UpdatePreview.PermissionSummary}").FontSize(12),
            new TextBlock().Text(_session.UpdatePreview.TrustWarning).FontSize(12));

        if (!_session.UpdatePreview.Success)
        {
            panel.Children(new TextBlock()
                .Text($"Update cannot continue: {_session.UpdatePreview.Message}")
                .FontSize(12));
            var selected = _controller.SelectedEntry();
            if (selected is not null)
            {
                panel.Children(new Button()
                    .Content("Choose Another Package")
                    .OnClick(async () => await _controller.ChooseUpdatePackageAsync(selected, workbench)));
            }

            return;
        }

        panel.Children(new StackPanel { Orientation = Orientation.Horizontal }
            .Spacing(6)
            .Children(
                ActionButton("Confirm Update", async () =>
                {
                    if (!await _controller.UpdatePreviewedPackageAsync(workbench))
                    {
                        RenderUpdatePreview(panel, workbench);
                    }
                }),
                new Button()
                    .Content("Choose Another Package")
                    .OnClick(async () =>
                    {
                        var selected = _controller.SelectedEntry();
                        if (selected is not null)
                        {
                            await _controller.ChooseUpdatePackageAsync(selected, workbench);
                        }
                    })));
    }

    private void RenderDetailsView(StackPanel panel, IWorkbenchService workbench)
    {
        panel.Clear();
        var entry = _controller.SelectedEntry();
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
            new TextBlock().Text($"Publisher: {PluginManagerFormatting.FormatPublisher(entry)}").FontSize(12),
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
                await _controller.EnableAsync(entry, workbench);
                RenderDetailsView(panel, workbench);
            }));
        }
        else if (entry.PluginId is not null && entry.State != MewooPluginManagerCatalogState.Broken)
        {
            actions.Children(ActionButton("Disable", async () =>
            {
                await _controller.DisableAsync(entry);
                RenderDetailsView(panel, workbench);
            }));
        }

        if (entry.PluginId is not null && entry.State != MewooPluginManagerCatalogState.Broken)
        {
            actions.Children(ActionButton("Update from File", async () =>
            {
                await _controller.ChooseUpdatePackageAsync(entry, workbench);
            }));
        }

        if (entry.State == MewooPluginManagerCatalogState.Broken)
        {
            actions.Children(ActionButton("Remove Broken Install", async () =>
            {
                await _controller.RemoveBrokenInstallAsync(entry);
                RenderDetailsView(panel, workbench);
            }));
        }
        else if (entry.PluginId is not null)
        {
            actions.Children(ActionButton("Uninstall", async () =>
            {
                await _controller.UninstallAsync(entry);
                RenderDetailsView(panel, workbench);
            }));
        }

        actions.Children(ActionButton("Open Diagnostics", async () =>
            await _controller.OpenDiagnosticsAsync(workbench)));

        return actions;
    }

    private StackPanel RecentOperationsBlock()
    {
        var block = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(4)
            .Children(new TextBlock().Text("Recent Operations").SemiBold().Margin(0, 10, 0, 0));
        if (_session.OperationResults.Count == 0)
        {
            block.Children(new TextBlock().Text("No plugin operations have run in this session.").FontSize(12));
            return block;
        }

        foreach (var operation in _session.OperationResults.Take(8))
        {
            block.Children(new TextBlock().Text(operation).FontSize(12));
        }

        return block;
    }

    private static Button ActionButton(string text, Func<Task> action)
    {
        return new Button()
            .Content(text)
            .OnClick(async () => await action());
    }

    private void AddCategoryAction(
        SidebarNavigationNodeBuilder manage,
        PluginManagerCategory category,
        IWorkbenchService workbench)
    {
        var label = PluginManagerFormatting.CategoryLabel(category);
        manage.Action(PluginManagerFormatting.CategoryId(category), category == _session.Category ? $"[{label}]" : label, async () =>
        {
            _session.Category = category;
            await workbench.OpenMainViewAsync("pluginManager.home");
        });
    }

    private string AdvancedLabel()
    {
        var issueCount = _controller.DiscoveryIssueCount + _controller.RuntimeProblemCount;
        return issueCount == 0 ? "Advanced" : $"Advanced ({issueCount})";
    }

    private string RuntimeStatusLabel()
    {
        var statusCount = _controller.RuntimeStatusCount;
        var problemCount = _controller.RuntimeProblemCount;
        if (problemCount > 0)
        {
            return $"Runtime Status ({problemCount})";
        }

        return statusCount == 0 ? "Runtime Status" : $"Runtime Status ({statusCount})";
    }

    private string DiscoveryIssuesLabel()
    {
        var issueCount = _controller.DiscoveryIssueCount;
        return issueCount == 0 ? "Discovery Issues" : $"Discovery Issues ({issueCount})";
    }
}
