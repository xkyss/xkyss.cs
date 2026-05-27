using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Mewoo.Abstractions;
using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Plugins;
using Mewoo.Abstractions.Views;

namespace Mewoo.Samples.RuntimeSmokePlugin;

public sealed class RuntimeSmokePlugin : IMewooPlugin
{
    private const string MainViewId = "mewoo.samples.runtimeSmoke.home";
    private int _openCount;

    public string Id => "mewoo.samples.runtimeSmoke";

    public string DisplayName => "Runtime Smoke Plugin";

    public void Register(IMewooContributionRegistry registry)
    {
        registry.Activity("mewoo.samples.runtimeSmoke.activity")
            .Title("Smoke")
            .Icon("S")
            .ViewContainer("mewoo.samples.runtimeSmoke.views")
            .Order(80);

        registry.ViewContainer("mewoo.samples.runtimeSmoke.views")
            .Title("Runtime Smoke")
            .AddView("mewoo.samples.runtimeSmoke.sidebar", view => view
                .Title("Smoke Sample")
                .Create(ctx => new MewooView("mewoo.samples.runtimeSmoke.sidebar", CreateSidebar(ctx.Workbench))));

        registry.MainView(MainViewId)
            .Title("Runtime Smoke")
            .CanOpenMultiple(false)
            .Create(_ => new MewooView(MainViewId, CreateMainView()));

        registry.Command("mewoo.samples.runtimeSmoke.open")
            .Title("Open Runtime Smoke")
            .Category("Runtime Smoke")
            .Execute(async (ctx, cancellationToken) =>
            {
                _openCount++;
                ctx.Workbench.UpdateStatusBarItem(
                    "mewoo.samples.runtimeSmoke.status",
                    $"Runtime Smoke: opened {_openCount}");
                await ctx.Workbench.OpenMainViewAsync(MainViewId, cancellationToken);
            });

        registry.StatusBarItem("mewoo.samples.runtimeSmoke.status")
            .AlignLeft()
            .Text("Runtime Smoke: loaded")
            .Command("mewoo.samples.runtimeSmoke.open");
    }

    private static StackPanel CreateSidebar(IWorkbenchService workbench)
    {
        return new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(8)
            .Margin(12)
            .Children(
                new TextBlock().Text("Runtime Smoke").SemiBold(),
                new TextBlock().Text("Loaded from %LocalAppData%\\Mewoo\\Plugins").FontSize(12),
                new Button()
                    .Content("Open Smoke View")
                    .OnClick(async () => await workbench.OpenMainViewAsync(MainViewId)));
    }

    private static StackPanel CreateMainView()
    {
        return new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(10)
            .Margin(18)
            .Children(
                new TextBlock().Text("Runtime Smoke Plugin").FontSize(22).SemiBold(),
                new TextBlock().Text("This view is provided by a runtime-loaded plugin package.").FontSize(13),
                new TextBlock().Text("If you can see this, manifest discovery, assembly loading, entryPoint instantiation, registration, activation, and view hosting all worked.").FontSize(12));
    }
}
