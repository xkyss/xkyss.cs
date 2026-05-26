using Mewoo.Abstractions.Contributions;

namespace Mewoo.Abstractions.Plugins;

public interface IMewooPlugin
{
    string Id { get; }

    string DisplayName { get; }

    void Register(IMewooContributionRegistry registry);
}

public interface IMewooPluginLifecycle
{
    ValueTask ActivateAsync(IMewooPluginContext context, CancellationToken cancellationToken);

    ValueTask DeactivateAsync(CancellationToken cancellationToken);

    ValueTask UnloadAsync(CancellationToken cancellationToken);

    ValueTask DisposeAsync();
}

public interface IMewooPluginContext
{
    string PluginId { get; }

    IServiceProvider Services { get; }

    IWorkbenchService Workbench { get; }
}

public enum MewooPluginState
{
    Created,
    Registered,
    Activated,
    Deactivated,
    Unloaded,
    Disposed,
    Failed,
}

