using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Plugins;
using Mewoo.Core.Commands;
using Mewoo.Core.Contributions;

namespace Mewoo.Core.Plugins;

public sealed class MewooPluginHost
{
    private readonly List<PluginEntry> _plugins = [];

    private readonly Dictionary<string, MewooContributionSnapshot> _registeredContributions = new(StringComparer.Ordinal);

    public event Action? ContributionsChanged;

    public MewooCommandRegistry Commands { get; } = new();

    public MewooContributionSnapshot VisibleContributions { get; private set; } =
        new([], [], [], [], [], []);

    public IReadOnlyList<PluginEntry> Plugins => _plugins;

    public PluginEntry RegisterPlugin(IMewooPlugin plugin)
    {
        var entry = new PluginEntry(plugin, MewooPluginState.Created, null);
        _plugins.Add(entry);

        try
        {
            var registry = new MewooContributionRegistry(plugin.Id);
            plugin.Register(registry);
            var snapshot = registry.BuildSnapshot();

            EnsureNoDuplicateVisibleOrRegistered(snapshot);
            _registeredContributions[plugin.Id] = snapshot;
            return UpdateEntry(plugin.Id, MewooPluginState.Registered, null);
        }
        catch (Exception ex)
        {
            return UpdateEntry(plugin.Id, MewooPluginState.Failed, ex);
        }
    }

    public async ValueTask ActivatePluginAsync(
        string pluginId,
        IMewooPluginContext context,
        CancellationToken cancellationToken = default)
    {
        var entry = FindEntry(pluginId);
        if (entry.State is MewooPluginState.Failed or MewooPluginState.Activated)
        {
            return;
        }

        try
        {
            if (entry.Plugin is IMewooPluginLifecycle lifecycle)
            {
                await lifecycle.ActivateAsync(context, cancellationToken);
            }

            AddVisibleContributions(pluginId);
            UpdateEntry(pluginId, MewooPluginState.Activated, null);
        }
        catch (Exception ex)
        {
            RemoveVisibleContributions(pluginId);
            UpdateEntry(pluginId, MewooPluginState.Failed, ex);
        }
    }

    public async ValueTask ActivateAllAsync(
        Func<IMewooPlugin, IMewooPluginContext> createContext,
        CancellationToken cancellationToken = default)
    {
        foreach (var entry in _plugins.ToArray())
        {
            await ActivatePluginAsync(entry.Plugin.Id, createContext(entry.Plugin), cancellationToken);
        }
    }

    public async ValueTask DeactivatePluginAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        var entry = FindEntry(pluginId);
        if (entry.State is not MewooPluginState.Activated)
        {
            return;
        }

        try
        {
            if (entry.Plugin is IMewooPluginLifecycle lifecycle)
            {
                await lifecycle.DeactivateAsync(cancellationToken);
            }

            RemoveVisibleContributions(pluginId);
            UpdateEntry(pluginId, MewooPluginState.Deactivated, null);
        }
        catch (Exception ex)
        {
            RemoveVisibleContributions(pluginId);
            UpdateEntry(pluginId, MewooPluginState.Failed, ex);
        }
    }

    public async ValueTask UnloadPluginAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        var entry = FindEntry(pluginId);

        try
        {
            if (entry.State is MewooPluginState.Activated)
            {
                await DeactivatePluginAsync(pluginId, cancellationToken);
            }

            if (entry.Plugin is IMewooPluginLifecycle lifecycle)
            {
                await lifecycle.UnloadAsync(cancellationToken);
            }

            RemoveVisibleContributions(pluginId);
            _registeredContributions.Remove(pluginId);
            UpdateEntry(pluginId, MewooPluginState.Unloaded, null);
        }
        catch (Exception ex)
        {
            RemoveVisibleContributions(pluginId);
            _registeredContributions.Remove(pluginId);
            UpdateEntry(pluginId, MewooPluginState.Failed, ex);
        }
    }

    public async ValueTask DisposePluginAsync(string pluginId)
    {
        var entry = FindEntry(pluginId);

        try
        {
            if (entry.State is MewooPluginState.Activated)
            {
                await DeactivatePluginAsync(pluginId);
            }

            if (entry.State is not MewooPluginState.Unloaded)
            {
                await UnloadPluginAsync(pluginId);
            }

            if (entry.Plugin is IMewooPluginLifecycle lifecycle)
            {
                await lifecycle.DisposeAsync();
            }

            UpdateEntry(pluginId, MewooPluginState.Disposed, null);
        }
        catch (Exception ex)
        {
            UpdateEntry(pluginId, MewooPluginState.Failed, ex);
        }
    }

    public MewooContributionSnapshot GetRegisteredContributions(string pluginId)
    {
        return _registeredContributions.TryGetValue(pluginId, out var snapshot)
            ? snapshot
            : new MewooContributionSnapshot([], [], [], [], [], []);
    }

    private PluginEntry FindEntry(string pluginId)
    {
        return _plugins.FirstOrDefault(entry => entry.Plugin.Id == pluginId)
            ?? throw new KeyNotFoundException($"Plugin id '{pluginId}' is not registered.");
    }

    private PluginEntry UpdateEntry(string pluginId, MewooPluginState state, Exception? error)
    {
        var index = _plugins.FindIndex(entry => entry.Plugin.Id == pluginId);
        if (index < 0)
        {
            throw new KeyNotFoundException($"Plugin id '{pluginId}' is not registered.");
        }

        var updated = _plugins[index] with { State = state, Error = error };
        _plugins[index] = updated;
        return updated;
    }

    private void AddVisibleContributions(string pluginId)
    {
        if (!_registeredContributions.TryGetValue(pluginId, out var snapshot))
        {
            return;
        }

        Commands.UnregisterOwner(pluginId);
        foreach (var command in snapshot.Commands)
        {
            Commands.Register(command);
        }

        VisibleContributions = Merge(RemoveOwner(VisibleContributions, pluginId), snapshot);
        ContributionsChanged?.Invoke();
    }

    private void RemoveVisibleContributions(string pluginId)
    {
        Commands.UnregisterOwner(pluginId);
        VisibleContributions = RemoveOwner(VisibleContributions, pluginId);
        ContributionsChanged?.Invoke();
    }

    private void EnsureNoDuplicateVisibleOrRegistered(MewooContributionSnapshot snapshot)
    {
        var withoutSameOwner = _registeredContributions.Values
            .Where(existing => existing.Activities.FirstOrDefault()?.OwnerPluginId != snapshot.Activities.FirstOrDefault()?.OwnerPluginId)
            .Aggregate(new MewooContributionSnapshot([], [], [], [], [], []), Merge);

        _ = Merge(withoutSameOwner, snapshot);
    }

    private static MewooContributionSnapshot RemoveOwner(MewooContributionSnapshot snapshot, string ownerPluginId)
    {
        return new MewooContributionSnapshot(
            snapshot.Activities.Where(x => x.OwnerPluginId != ownerPluginId).ToArray(),
            snapshot.ViewContainers.Where(x => x.OwnerPluginId != ownerPluginId).ToArray(),
            snapshot.MainViews.Where(x => x.OwnerPluginId != ownerPluginId).ToArray(),
            snapshot.Commands.Where(x => x.OwnerPluginId != ownerPluginId).ToArray(),
            snapshot.StatusBarItems.Where(x => x.OwnerPluginId != ownerPluginId).ToArray(),
            snapshot.ThemeTokenOverrides.Where(x => x.OwnerPluginId != ownerPluginId).ToArray());
    }

    private static MewooContributionSnapshot Merge(MewooContributionSnapshot left, MewooContributionSnapshot right)
    {
        var merged = new MewooContributionSnapshot(
            left.Activities.Concat(right.Activities).ToArray(),
            left.ViewContainers.Concat(right.ViewContainers).ToArray(),
            left.MainViews.Concat(right.MainViews).ToArray(),
            left.Commands.Concat(right.Commands).ToArray(),
            left.StatusBarItems.Concat(right.StatusBarItems).ToArray(),
            left.ThemeTokenOverrides.Concat(right.ThemeTokenOverrides).ToArray());

        var duplicate = merged.Activities.Select(x => x.Id)
            .Concat(merged.ViewContainers.Select(x => x.Id))
            .Concat(merged.ViewContainers.SelectMany(x => x.Views).Select(x => x.Id))
            .Concat(merged.MainViews.Select(x => x.Id))
            .Concat(merged.Commands.Select(x => x.Id))
            .Concat(merged.StatusBarItems.Select(x => x.Id))
            .Concat(merged.ThemeTokenOverrides.Select(x => x.Id))
            .GroupBy(x => x, StringComparer.Ordinal)
            .FirstOrDefault(x => x.Count() > 1);

        if (duplicate is not null)
        {
            throw new InvalidOperationException($"Contribution id '{duplicate.Key}' is registered more than once.");
        }

        return merged;
    }
}

public sealed record PluginEntry(IMewooPlugin Plugin, MewooPluginState State, Exception? Error);
