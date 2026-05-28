using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Plugins;
using Mewoo.Abstractions.Views;

namespace Mewoo.TestPlugins.ValidRuntimePlugin;

public sealed class ValidRuntimePlugin : IMewooPlugin
{
    public const string PluginId = "xkyss.validRuntimePlugin";

    public string Id => PluginId;

    public string DisplayName => "Valid Runtime Plugin";

    public void Register(IMewooContributionRegistry registry)
    {
        registry.Activity("xkyss.validRuntimePlugin.activity")
            .Title("Valid Runtime")
            .Icon("V")
            .ViewContainer("xkyss.validRuntimePlugin.views")
            .Order(20);

        registry.ViewContainer("xkyss.validRuntimePlugin.views")
            .Title("Valid Runtime")
            .AddView("xkyss.validRuntimePlugin.sidebar", view => view
                .Title("Runtime Sidebar")
                .Create(_ => new MewooView("xkyss.validRuntimePlugin.sidebar", new object())));

        registry.MainView("xkyss.validRuntimePlugin.main")
            .Title("Runtime Main")
            .CanOpenMultiple(false)
            .Create(_ => new MewooView("xkyss.validRuntimePlugin.main", new object()));

        registry.Command("xkyss.validRuntimePlugin.ping")
            .Title("Runtime Plugin Ping")
            .Category("Runtime Plugin")
            .Execute((_, _) => ValueTask.CompletedTask);

        registry.StatusBarItem("xkyss.validRuntimePlugin.status")
            .AlignLeft()
            .Text("Runtime plugin loaded");
    }
}

public sealed class DifferentIdRuntimePlugin : IMewooPlugin
{
    public string Id => "xkyss.differentRuntimePlugin";

    public string DisplayName => "Different Runtime Plugin";

    public void Register(IMewooContributionRegistry registry)
    {
    }
}

public sealed class ThrowingRuntimePlugin : IMewooPlugin
{
    public ThrowingRuntimePlugin()
    {
        throw new InvalidOperationException("Constructor failed.");
    }

    public string Id => "xkyss.throwingRuntimePlugin";

    public string DisplayName => "Throwing Runtime Plugin";

    public void Register(IMewooContributionRegistry registry)
    {
    }
}

public sealed class ActivatingRuntimePlugin : IMewooPlugin, IMewooPluginLifecycle
{
    public string Id => "xkyss.activatingRuntimePlugin";

    public string DisplayName => "Activating Runtime Plugin";

    public void Register(IMewooContributionRegistry registry)
    {
    }

    public ValueTask ActivateAsync(IMewooPluginContext context, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Activation failed.");
    }

    public ValueTask DeactivateAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;

    public ValueTask UnloadAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

public sealed class NotAPlugin
{
}
