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
    private readonly string _pluginRoot;
    private readonly MewooPluginManagerCatalog _catalog = new();

    private string _searchText = string.Empty;
    private MewooPluginManagerCatalogFilter _filter = MewooPluginManagerCatalogFilter.All;

    public PluginManagerPlugin(MewooRuntimePluginManager runtimePlugins, string pluginRoot)
    {
        _runtimePlugins = runtimePlugins;
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
            panel.Children(PluginRow(entry));
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

    private Border PluginRow(MewooPluginManagerCatalogEntry entry)
    {
        return new Border()
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
                        .Text($"Use Install from File to add a local {MewooPluginPackageFormat.Extension} package when the install flow is available.")
                        .FontSize(12)));
    }

    private IReadOnlyList<MewooPluginManagerCatalogEntry> Entries() =>
        _catalog.CreateEntries(
            _pluginRoot,
            _runtimePlugins.PluginStatuses,
            _runtimePlugins.DiscoveryIssues,
            new MewooPluginManagerCatalogQuery(_searchText, _filter));

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
}
