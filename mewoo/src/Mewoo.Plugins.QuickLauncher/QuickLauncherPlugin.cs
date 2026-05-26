using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Plugins;
using Mewoo.Abstractions.Views;

namespace Mewoo.Plugins.QuickLauncher;

public sealed class QuickLauncherPlugin : IMewooPlugin
{
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
                .Create(ctx => new MewooView("quickLauncher.shortcuts", CreateSidebar())));

        registry.MainView("quickLauncher.home")
            .Title("Launcher")
            .CanOpenMultiple(false)
            .Create(ctx => new MewooView("quickLauncher.home", CreateMainView()));

        registry.Command("quickLauncher.open")
            .Title("Open Launcher")
            .Category("Quick Launcher")
            .Execute(async (ctx, cancellationToken) =>
                await ctx.Workbench.OpenMainViewAsync("quickLauncher.home", cancellationToken));

        registry.StatusBarItem("quickLauncher.status")
            .AlignLeft()
            .Text("QuickLauncher: ready");
    }

    private static StackPanel CreateSidebar()
    {
        return new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(8)
            .Margin(12)
            .Children(
                new TextBox().Placeholder("Search shortcuts"),
                new TextBlock().Text("Groups").SemiBold(),
                new TextBlock().Text("- Development"),
                new TextBlock().Text("- Trading"),
                new TextBlock().Text("- Writing"),
                new TextBlock().Text("Recent").SemiBold().Margin(0, 12, 0, 0),
                new TextBlock().Text("- GitHub"),
                new TextBlock().Text("- Terminal"));
    }

    private static StackPanel CreateMainView()
    {
        return new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(10)
            .Margin(18)
            .Children(
                new TextBlock().Text("Launcher").FontSize(22).SemiBold(),
                new TextBox().Placeholder("Search websites, apps, scripts..."),
                new TextBlock().Text("Development").SemiBold().Margin(0, 10, 0, 0),
                LauncherRow("GitHub", "https://github.com"),
                LauncherRow("Terminal", "wt.exe"),
                LauncherRow("Build Script", "scripts/build.ps1"),
                new TextBlock().Text("Trading").SemiBold().Margin(0, 10, 0, 0),
                LauncherRow("Binance", "https://www.binance.com"));
    }

    private static Border LauncherRow(string title, string target)
    {
        return new Border()
            .Padding(10, 6)
            .Child(new DockPanel().Children(
                new TextBlock().DockRight().Text(target),
                new TextBlock().Text(title).SemiBold()));
    }
}
