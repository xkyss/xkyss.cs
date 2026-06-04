using System.Reflection;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Mewoo.Abstractions;
using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Plugins;
using Mewoo.Abstractions.Views;
using Mewoo.Controls.Sidebar;

namespace Mewoo.Plugins.Settings;

public sealed class SettingsPlugin : IMewooPlugin
{
    public string Id => "settings";

    public string DisplayName => "Settings";

    public void Register(IMewooContributionRegistry registry)
    {
        registry.Activity("settings.activity")
            .Title("Settings")
            .Icon("S")
            .ViewContainer("settings.views")
            .ActivityBarSection(ActivityBarSection.System)
            .Order(90);

        registry.ViewContainer("settings.views")
            .Title("Settings")
            .AddView("settings.general", view => view
                .Title("General")
                .Create(ctx => new MewooView("settings.general", CreateSidebar(ctx.Workbench))));

        registry.MainView("settings.home")
            .Title("Settings")
            .CanOpenMultiple(false)
            .Create(ctx => new MewooView("settings.home", CreateMainView(ctx.Workbench)));

        registry.Command("settings.open")
            .Title("Open Settings")
            .Category("Settings")
            .Execute(async (ctx, cancellationToken) =>
                await ctx.Workbench.OpenMainViewAsync("settings.home", cancellationToken));
    }

    private static StackPanel CreateSidebar(IWorkbenchService workbench)
    {
        var navigation = SidebarNavigation.Create(workbench)
            .MainView("settings.general", "General", "settings.home")
            .Action("settings.appearance", "Appearance", () =>
                workbench.UpdateStatusBarItem("settings.status", "Settings: appearance opened"))
            .MainView("settings.plugins", "Plugins", "pluginManager.home")
            .Build()
            .Margin(4, 0, 4, 0);

        return SidebarLayout.Create()
            .Header(SidebarHeader.Create("Settings")
                .More(menu => menu.Item("Open Settings", async () =>
                    await workbench.OpenMainViewAsync("settings.home"))))
            .Body(navigation)
            .Build();
    }

    private static StackPanel CreateMainView(IWorkbenchService workbench)
    {
        return new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(10)
            .Margin(18)
            .Children(
                new TextBlock().Text("Settings").FontSize(20).SemiBold(),
                new TextBlock().Text($"Mewoo {VersionLabel()}").FontSize(12),
                Section(
                    "Appearance",
                    "Theme and visual preferences will live here.",
                    new Button()
                        .Content("Toggle Theme")
                        .OnClick(() => workbench.UpdateStatusBarItem("settings.status", "Settings: appearance opened"))),
                Section(
                    "Plugins",
                    "Manage installed plugins from Plugin Manager.",
                    new Button()
                        .Content("Open Plugin Manager")
                        .OnClick(async () => await workbench.OpenMainViewAsync("pluginManager.home"))),
                Section(
                    "Future Settings",
                    "A structured settings schema and plugin-contributed settings pages are reserved for a later version."));
    }

    private static StackPanel Section(string title, string description, params Element[] actions)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(6)
            .Margin(0, 4, 0, 8)
            .Children(
                new TextBlock().Text(title).SemiBold(),
                new TextBlock().Text(description).FontSize(12));

        foreach (var action in actions)
        {
            panel.Children(action);
        }

        return panel;
    }

    private static string VersionLabel()
    {
        return typeof(SettingsPlugin).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "0.6.0-dev";
    }
}
