using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Mewoo.Abstractions;
using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Plugins;
using Mewoo.Abstractions.Views;

namespace Mewoo.Plugins.QuickLauncher;

public sealed class QuickLauncherPlugin : IMewooPlugin
{
    private QuickLauncherConfig _config = QuickLauncherConfig.LoadOrCreateDefault();
    private QuickLauncherItem? _selectedItem;
    private TextBox? _mainSearchBox;
    private TextBox? _sidebarSearchBox;

    public string Id => "quickLauncher";

    public string DisplayName => "Quick Launcher";

    public void Register(IMewooContributionRegistry registry)
    {
        registry.Activity("quickLauncher.activity")
            .Title("Launcher")
            .Icon("L")
            .ViewContainer("quickLauncher.views")
            .Order(0);

        registry.ViewContainer("quickLauncher.views")
            .Title("Launcher")
            .AddView("quickLauncher.shortcuts", view => view
            .Title("Shortcuts")
                .Create(ctx => new MewooView("quickLauncher.shortcuts", CreateSidebar(ctx.Workbench))));

        registry.MainView("quickLauncher.home")
            .Title("Launcher")
            .CanOpenMultiple(false)
            .Create(ctx => new MewooView("quickLauncher.home", CreateMainView(ctx.Workbench)));

        registry.Command("quickLauncher.open")
            .Title("Open Launcher")
            .Category("Quick Launcher")
            .Execute(async (ctx, cancellationToken) =>
                await ctx.Workbench.OpenMainViewAsync("quickLauncher.home", cancellationToken));

        registry.Command("quickLauncher.focusSearch")
            .Title("Focus Launcher Search")
            .Category("Quick Launcher")
            .Execute(async (ctx, cancellationToken) =>
            {
                await ctx.Workbench.OpenMainViewAsync("quickLauncher.home", cancellationToken);
                _mainSearchBox?.Focus();
                SetStatus(ctx.Workbench, "Search focused");
            });

        registry.Command("quickLauncher.runSelected")
            .Title("Run Selected Launcher Item")
            .Category("Quick Launcher")
            .CanExecute(_ => GetSelectedOrDefault() is not null)
            .Execute((ctx, _) =>
            {
                var selected = GetSelectedOrDefault();
                if (selected is null)
                {
                    SetStatus(ctx.Workbench, "No launcher item selected");
                    return ValueTask.CompletedTask;
                }

                RunItem(selected, ctx.Workbench);

                return ValueTask.CompletedTask;
            });

        registry.Command("quickLauncher.reload")
            .Title("Reload Launcher Config")
            .Category("Quick Launcher")
            .Execute(async (ctx, cancellationToken) =>
            {
                _config = QuickLauncherConfig.LoadOrCreateDefault();
                _selectedItem = null;
                SetStatus(ctx.Workbench, $"Reloaded: {_config.Groups.Sum(group => group.Items.Count)} items");
                await ctx.Workbench.OpenMainViewAsync("quickLauncher.home", cancellationToken);
            });

        registry.Command("quickLauncher.openConfig")
            .Title("Open Launcher Config")
            .Category("Quick Launcher")
            .Execute((ctx, _) =>
            {
                try
                {
                    QuickLauncherLaunchService.OpenConfig();
                    SetStatus(ctx.Workbench, "Opened launcher config");
                }
                catch (Exception ex)
                {
                    SetStatus(ctx.Workbench, $"Open config failed: {ex.Message}");
                }

                return ValueTask.CompletedTask;
            });

        registry.StatusBarItem("quickLauncher.status")
            .AlignLeft()
            .Text($"QuickLauncher: {_config.Groups.Sum(group => group.Items.Count)} items");
    }

    private StackPanel CreateSidebar(IWorkbenchService workbench)
    {
        var recentItems = _config.Groups
            .SelectMany(group => group.Items)
            .Take(5)
            .ToArray();

        _sidebarSearchBox = new TextBox().Placeholder("Search shortcuts");

        return new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(8)
            .Margin(12)
            .Children(
                _sidebarSearchBox,
                new TextBlock().Text("Groups").SemiBold(),
                new StackPanel { Orientation = Orientation.Vertical }
                    .Spacing(4)
                    .Children(_config.Groups
                        .Select(group => SidebarGroupItem(group, workbench) as Element)
                        .ToArray()),
                new TextBlock().Text("Recent").SemiBold().Margin(0, 12, 0, 0),
                new StackPanel { Orientation = Orientation.Vertical }
                    .Spacing(4)
                    .Children(recentItems
                        .Select(item => SidebarRecentItem(item, workbench) as Element)
                        .ToArray()));
    }

    private StackPanel CreateMainView(IWorkbenchService workbench)
    {
        var content = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(10)
            .Margin(18);

        _mainSearchBox = new TextBox().Placeholder("Search websites, apps, scripts...");

        content.Children(
            new TextBlock().Text("Launcher").FontSize(22).SemiBold(),
            _mainSearchBox,
            new TextBlock()
                .Text($"Config: {QuickLauncherConfig.GetDefaultConfigPath()}")
                .FontSize(11));

        foreach (var group in _config.Groups)
        {
            content.Children(new TextBlock().Text(group.Title).SemiBold().Margin(0, 10, 0, 0));

            foreach (var item in group.Items)
            {
                content.Children(LauncherRow(item, workbench));
            }
        }

        return content;
    }

    private Border LauncherRow(QuickLauncherItem item, IWorkbenchService workbench)
    {
        var content = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(2)
            .Children(
                new DockPanel().Children(
                    new TextBlock().DockRight().Text(item.Kind.ToString()).FontSize(11),
                    new TextBlock().Text(item.Title).SemiBold()),
                new TextBlock().Text(item.Target).FontSize(11));

        return new Border()
            .Padding(10, 6)
            .Child(content)
            .OnMouseDown(e =>
            {
                if (e.Button != MouseButton.Left)
                {
                    return;
                }

                SelectItem(item, workbench);
                if (e.ClickCount >= 2)
                {
                    RunItem(item, workbench);
                }

                e.Handled = true;
            });
    }

    private Border SidebarGroupItem(QuickLauncherGroup group, IWorkbenchService workbench)
    {
        var itemCount = group.Items.Count;
        var groupItem = new Border()
            .Padding(6, 3)
            .ToolTip($"{itemCount} items")
            .Child(new DockPanel().Children(
                new TextBlock().DockRight().Text(itemCount.ToString()).FontSize(11),
                new TextBlock().Text(group.Title)));

        groupItem.MouseDown += e =>
        {
            if (e.Button != MouseButton.Left)
            {
                return;
            }

            _ = workbench.OpenMainViewAsync("quickLauncher.home").AsTask();
            if (group.Items.FirstOrDefault() is { } firstItem)
            {
                SelectItem(firstItem, workbench);
            }
            else
            {
                SetStatus(workbench, $"Group: {group.Title} is empty");
            }

            e.Handled = true;
        };

        return groupItem;
    }

    private Border SidebarRecentItem(QuickLauncherItem item, IWorkbenchService workbench)
    {
        var row = new Border()
            .Padding(6, 3)
            .Child(new TextBlock().Text(item.Title));

        row.MouseDown += e =>
        {
            if (e.Button != MouseButton.Left)
            {
                return;
            }

            SelectItem(item, workbench);
            if (e.ClickCount >= 2)
            {
                RunItem(item, workbench);
            }

            e.Handled = true;
        };

        return row;
    }

    private QuickLauncherItem? GetSelectedOrDefault()
    {
        return _selectedItem ?? _config.Groups.SelectMany(group => group.Items).FirstOrDefault();
    }

    private void SelectItem(QuickLauncherItem item, IWorkbenchService workbench)
    {
        _selectedItem = item;
        SetStatus(workbench, $"Selected: {item.Title}");
    }

    private static void RunItem(QuickLauncherItem item, IWorkbenchService workbench)
    {
        try
        {
            QuickLauncherLaunchService.Run(item);
            SetStatus(workbench, $"Ran: {item.Title}");
        }
        catch (Exception ex)
        {
            SetStatus(workbench, $"Run failed: {ex.Message}");
        }
    }

    private static void SetStatus(IWorkbenchService workbench, string text)
    {
        workbench.UpdateStatusBarItem("quickLauncher.status", $"QuickLauncher: {text}");
    }
}
