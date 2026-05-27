using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Plugins;
using Mewoo.Abstractions.Views;

namespace Mewoo.Plugins.QuickLauncher;

public sealed class QuickLauncherPlugin : IMewooPlugin
{
    private readonly QuickLauncherConfig _config = QuickLauncherConfig.LoadOrCreateDefault();

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
                .Create(ctx => new MewooView("quickLauncher.shortcuts", CreateSidebar(_config))));

        registry.MainView("quickLauncher.home")
            .Title("Launcher")
            .CanOpenMultiple(false)
            .Create(ctx => new MewooView("quickLauncher.home", CreateMainView(_config)));

        registry.Command("quickLauncher.open")
            .Title("Open Launcher")
            .Category("Quick Launcher")
            .Execute(async (ctx, cancellationToken) =>
                await ctx.Workbench.OpenMainViewAsync("quickLauncher.home", cancellationToken));

        registry.StatusBarItem("quickLauncher.status")
            .AlignLeft()
            .Text($"QuickLauncher: {_config.Groups.Sum(group => group.Items.Count)} items");
    }

    private static StackPanel CreateSidebar(QuickLauncherConfig config)
    {
        var recentItems = config.Groups
            .SelectMany(group => group.Items)
            .Take(5)
            .ToArray();

        return new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(8)
            .Margin(12)
            .Children(
                new TextBox().Placeholder("Search shortcuts"),
                new TextBlock().Text("Groups").SemiBold(),
                new StackPanel { Orientation = Orientation.Vertical }
                    .Spacing(4)
                    .Children(config.Groups
                        .Select(group => new TextBlock().Text($"- {group.Title}") as Element)
                        .ToArray()),
                new TextBlock().Text("Recent").SemiBold().Margin(0, 12, 0, 0),
                new StackPanel { Orientation = Orientation.Vertical }
                    .Spacing(4)
                    .Children(recentItems
                        .Select(item => new TextBlock().Text($"- {item.Title}") as Element)
                        .ToArray()));
    }

    private static StackPanel CreateMainView(QuickLauncherConfig config)
    {
        var content = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(10)
            .Margin(18);

        content.Children(
            new TextBlock().Text("Launcher").FontSize(22).SemiBold(),
            new TextBox().Placeholder("Search websites, apps, scripts..."),
            new TextBlock()
                .Text($"Config: {QuickLauncherConfig.GetDefaultConfigPath()}")
                .FontSize(11));

        foreach (var group in config.Groups)
        {
            content.Children(new TextBlock().Text(group.Title).SemiBold().Margin(0, 10, 0, 0));

            foreach (var item in group.Items)
            {
                content.Children(LauncherRow(item));
            }
        }

        return content;
    }

    private static Border LauncherRow(QuickLauncherItem item)
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
            .Child(content);
    }
}
