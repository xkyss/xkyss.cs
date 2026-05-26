using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Plugins;
using Mewoo.Core.Commands;
using Mewoo.Core.Contributions;

namespace Mewoo.Core.Plugins;

public sealed class MewooPluginHost
{
    private readonly List<PluginEntry> _plugins = [];

    public MewooCommandRegistry Commands { get; } = new();

    public MewooContributionSnapshot Contributions { get; private set; } =
        new([], [], [], [], [], []);

    public IReadOnlyList<PluginEntry> Plugins => _plugins;

    public void RegisterPlugin(IMewooPlugin plugin)
    {
        var registry = new MewooContributionRegistry(plugin.Id);
        plugin.Register(registry);
        var snapshot = registry.BuildSnapshot();

        foreach (var command in snapshot.Commands)
        {
            Commands.Register(command);
        }

        Contributions = Merge(Contributions, snapshot);
        _plugins.Add(new PluginEntry(plugin, MewooPluginState.Registered, null));
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

