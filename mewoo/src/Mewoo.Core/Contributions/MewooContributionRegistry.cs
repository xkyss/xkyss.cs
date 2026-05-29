using System.Text.RegularExpressions;
using Mewoo.Abstractions.Commands;
using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Views;

namespace Mewoo.Core.Contributions;

#pragma warning disable CS9124

public sealed partial class MewooContributionRegistry(string pluginId) : IMewooContributionRegistry
{
    private readonly List<ActivityBuilder> _activities = [];
    private readonly List<ViewContainerBuilder> _viewContainers = [];
    private readonly List<MainViewBuilder> _mainViews = [];
    private readonly List<CommandBuilder> _commands = [];
    private readonly List<StatusBarItemBuilder> _statusBarItems = [];
    private readonly List<ThemeTokenOverrideBuilder> _themeTokenOverrides = [];

    public IActivityContributionBuilder Activity(string id)
    {
        ValidateId(id);
        var builder = new ActivityBuilder(pluginId, id);
        _activities.Add(builder);
        return builder;
    }

    public IViewContainerContributionBuilder ViewContainer(string id)
    {
        ValidateId(id);
        var builder = new ViewContainerBuilder(pluginId, id);
        _viewContainers.Add(builder);
        return builder;
    }

    public IMainViewContributionBuilder MainView(string id)
    {
        ValidateId(id);
        var builder = new MainViewBuilder(pluginId, id);
        _mainViews.Add(builder);
        return builder;
    }

    public ICommandContributionBuilder Command(string id)
    {
        ValidateId(id);
        var builder = new CommandBuilder(pluginId, id);
        _commands.Add(builder);
        return builder;
    }

    public IStatusBarItemContributionBuilder StatusBarItem(string id)
    {
        ValidateId(id);
        var builder = new StatusBarItemBuilder(pluginId, id);
        _statusBarItems.Add(builder);
        return builder;
    }

    public IThemeTokenOverrideContributionBuilder ThemeTokenOverride(string id)
    {
        ValidateId(id);
        var builder = new ThemeTokenOverrideBuilder(pluginId, id);
        _themeTokenOverrides.Add(builder);
        return builder;
    }

    public MewooContributionSnapshot BuildSnapshot()
    {
        var activities = _activities.Select(x => x.Build()).ToArray();
        var activityIds = activities.Select(x => x.Id).ToArray();
        var viewContainerScopes = InferViewContainerScopes(activities, _viewContainers.Select(x => x.Id).ToArray());

        var snapshot = new MewooContributionSnapshot(
            activities,
            _viewContainers.Select(x => x.Build(viewContainerScopes[x.Id])).ToArray(),
            _mainViews.Select(x => x.Build(InferActivityScope(x.Id, x.ActivityScopeId, x.IsGlobal, activityIds, "Main view"))).ToArray(),
            _commands.Select(x => x.Build()).ToArray(),
            _statusBarItems.Select(x => x.Build(InferActivityScope(x.Id, x.ActivityScopeId, x.IsGlobal, activityIds, "StatusBar item"))).ToArray(),
            _themeTokenOverrides.Select(x => x.Build()).ToArray());

        ValidateUnique(snapshot);
        return snapshot;
    }

    private static IReadOnlyDictionary<string, string> InferViewContainerScopes(
        IReadOnlyList<ActivityDescriptor> activities,
        IReadOnlyList<string> viewContainerIds)
    {
        var scopes = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var group in activities.GroupBy(activity => activity.ViewContainerId, StringComparer.Ordinal))
        {
            var activityIds = group.Select(activity => activity.Id).ToArray();
            if (activityIds.Length > 1)
            {
                throw new InvalidOperationException(
                    $"View container '{group.Key}' is used by multiple activities and cannot infer a single Activity scope.");
            }

            scopes[group.Key] = activityIds[0];
        }

        foreach (var viewContainerId in viewContainerIds)
        {
            if (scopes.ContainsKey(viewContainerId))
            {
                continue;
            }

            scopes[viewContainerId] = activities.Count switch
            {
                1 => activities[0].Id,
                0 => throw new InvalidOperationException(
                    $"View container '{viewContainerId}' requires an Activity scope, but the plugin does not contribute an Activity."),
                _ => throw new InvalidOperationException(
                    $"View container '{viewContainerId}' requires an explicit Activity scope because the plugin contributes multiple Activities."),
            };
        }

        return scopes;
    }

    private static InferredContributionScope InferActivityScope(
        string contributionId,
        string? activityScopeId,
        bool isGlobal,
        IReadOnlyList<string> activityIds,
        string contributionKind)
    {
        if (isGlobal)
        {
            return new InferredContributionScope(null, true);
        }

        if (!string.IsNullOrWhiteSpace(activityScopeId))
        {
            if (!activityIds.Contains(activityScopeId, StringComparer.Ordinal))
            {
                throw new InvalidOperationException(
                    $"{contributionKind} '{contributionId}' declares Activity scope '{activityScopeId}', but that Activity is not contributed by this plugin.");
            }

            return new InferredContributionScope(activityScopeId, false);
        }

        return activityIds.Count switch
        {
            1 => new InferredContributionScope(activityIds[0], false),
            0 => throw new InvalidOperationException(
                $"{contributionKind} '{contributionId}' requires an Activity scope or explicit global scope."),
            _ => throw new InvalidOperationException(
                $"{contributionKind} '{contributionId}' requires an explicit Activity scope because the plugin contributes multiple Activities."),
        };
    }

    private static void ValidateUnique(MewooContributionSnapshot snapshot)
    {
        var ids = snapshot.Activities.Select(x => x.Id)
            .Concat(snapshot.ViewContainers.Select(x => x.Id))
            .Concat(snapshot.ViewContainers.SelectMany(x => x.Views).Select(x => x.Id))
            .Concat(snapshot.MainViews.Select(x => x.Id))
            .Concat(snapshot.Commands.Select(x => x.Id))
            .Concat(snapshot.StatusBarItems.Select(x => x.Id))
            .Concat(snapshot.ThemeTokenOverrides.Select(x => x.Id));

        var duplicate = ids.GroupBy(x => x, StringComparer.Ordinal).FirstOrDefault(x => x.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException($"Contribution id '{duplicate.Key}' is registered more than once.");
        }
    }

    private static void ValidateId(string id)
    {
        if (!ContributionIdRegex().IsMatch(id))
        {
            throw new ArgumentException($"Contribution id '{id}' must be namespaced, for example 'quickLauncher.open'.", nameof(id));
        }
    }

    [GeneratedRegex("^[a-z][a-zA-Z0-9]*(\\.[a-z][a-zA-Z0-9]*)+$")]
    private static partial Regex ContributionIdRegex();

    private sealed class ActivityBuilder(string ownerPluginId, string id) : IActivityContributionBuilder
    {
        private string _title = id;
        private string? _icon;
        private string? _viewContainerId;
        private int _order;

        public IActivityContributionBuilder Title(string title) { _title = title; return this; }
        public IActivityContributionBuilder Icon(string icon) { _icon = icon; return this; }
        public IActivityContributionBuilder ViewContainer(string viewContainerId) { ValidateId(viewContainerId); _viewContainerId = viewContainerId; return this; }
        public IActivityContributionBuilder Order(int order) { _order = order; return this; }

        public ActivityDescriptor Build() => new(id, ownerPluginId, _title, _icon, _viewContainerId ?? throw new InvalidOperationException($"Activity '{id}' requires a view container."), _order);
    }

    private sealed class ViewContainerBuilder(string ownerPluginId, string id) : IViewContainerContributionBuilder
    {
        private readonly List<SidebarViewBuilder> _views = [];
        private string _title = id;

        public string Id => id;

        public IViewContainerContributionBuilder Title(string title) { _title = title; return this; }

        public IViewContainerContributionBuilder AddView(string viewId, Action<ISidebarViewContributionBuilder> configure)
        {
            ValidateId(viewId);
            var builder = new SidebarViewBuilder(ownerPluginId, viewId);
            configure(builder);
            _views.Add(builder);
            return this;
        }

        public ViewContainerDescriptor Build(string activityScopeId) =>
            new(id, ownerPluginId, activityScopeId, _title, _views.Select(x => x.Build(activityScopeId)).ToArray());
    }

    private sealed class SidebarViewBuilder(string ownerPluginId, string id) : ISidebarViewContributionBuilder
    {
        private string _title = id;
        private Func<IMewooViewContext, IMewooView>? _create;

        public ISidebarViewContributionBuilder Title(string title) { _title = title; return this; }
        public ISidebarViewContributionBuilder Create(Func<IMewooViewContext, IMewooView> createView) { _create = createView; return this; }

        public SidebarViewDescriptor Build(string activityScopeId) =>
            new(id, ownerPluginId, activityScopeId, _title, _create ?? throw new InvalidOperationException($"Sidebar view '{id}' requires a factory."));
    }

    private sealed class MainViewBuilder(string ownerPluginId, string id) : IMainViewContributionBuilder
    {
        private string _title = id;
        private string? _activityScopeId;
        private bool _isGlobal;
        private bool _canOpenMultiple;
        private Func<IMewooViewContext, IMewooView>? _create;

        public string Id => id;

        public IMainViewContributionBuilder Title(string title) { _title = title; return this; }
        public IMainViewContributionBuilder ActivityScope(string activityId) { ValidateId(activityId); _activityScopeId = activityId; _isGlobal = false; return this; }
        public IMainViewContributionBuilder Global() { _activityScopeId = null; _isGlobal = true; return this; }
        public IMainViewContributionBuilder CanOpenMultiple(bool canOpenMultiple) { _canOpenMultiple = canOpenMultiple; return this; }
        public IMainViewContributionBuilder Create(Func<IMewooViewContext, IMewooView> createView) { _create = createView; return this; }

        public string? ActivityScopeId => _activityScopeId;

        public bool IsGlobal => _isGlobal;

        public MainViewDescriptor Build(InferredContributionScope scope) =>
            new(id, ownerPluginId, scope.ActivityScopeId, scope.IsGlobal, _title, _canOpenMultiple, _create ?? throw new InvalidOperationException($"Main view '{id}' requires a factory."));
    }

    private sealed class CommandBuilder(string ownerPluginId, string id) : ICommandContributionBuilder
    {
        private string _title = id;
        private string? _category;
        private string? _icon;
        private string? _defaultKeyBinding;
        private Func<IMewooCommandContext, bool>? _canExecute;
        private Func<IMewooCommandContext, CancellationToken, ValueTask>? _execute;

        public ICommandContributionBuilder Title(string title) { _title = title; return this; }
        public ICommandContributionBuilder Category(string category) { _category = category; return this; }
        public ICommandContributionBuilder Icon(string icon) { _icon = icon; return this; }
        public ICommandContributionBuilder DefaultKeyBinding(string defaultKeyBinding) { _defaultKeyBinding = defaultKeyBinding; return this; }
        public ICommandContributionBuilder CanExecute(Func<IMewooCommandContext, bool> canExecute) { _canExecute = canExecute; return this; }
        public ICommandContributionBuilder Execute(Func<IMewooCommandContext, CancellationToken, ValueTask> executeAsync) { _execute = executeAsync; return this; }

        public MewooCommandDescriptor Build() => new(id, ownerPluginId, _title, _category, _icon, _defaultKeyBinding, _execute ?? throw new InvalidOperationException($"Command '{id}' requires an executor."), _canExecute);
    }

    private sealed class StatusBarItemBuilder(string ownerPluginId, string id) : IStatusBarItemContributionBuilder
    {
        private StatusBarAlignment _alignment = StatusBarAlignment.Left;
        private string _text = id;
        private string? _commandId;
        private string? _activityScopeId;
        private bool _isGlobal;

        public string Id => id;

        public IStatusBarItemContributionBuilder ActivityScope(string activityId) { ValidateId(activityId); _activityScopeId = activityId; _isGlobal = false; return this; }
        public IStatusBarItemContributionBuilder Global() { _activityScopeId = null; _isGlobal = true; return this; }
        public IStatusBarItemContributionBuilder AlignLeft() { _alignment = StatusBarAlignment.Left; return this; }
        public IStatusBarItemContributionBuilder AlignRight() { _alignment = StatusBarAlignment.Right; return this; }
        public IStatusBarItemContributionBuilder Text(string text) { _text = text; return this; }
        public IStatusBarItemContributionBuilder Command(string commandId) { ValidateId(commandId); _commandId = commandId; return this; }

        public string? ActivityScopeId => _activityScopeId;

        public bool IsGlobal => _isGlobal;

        public StatusBarItemDescriptor Build(InferredContributionScope scope) =>
            new(id, ownerPluginId, scope.ActivityScopeId, scope.IsGlobal, _alignment, _text, _commandId);
    }

    private sealed class ThemeTokenOverrideBuilder(string ownerPluginId, string id) : IThemeTokenOverrideContributionBuilder
    {
        private readonly Dictionary<string, object> _tokens = new(StringComparer.Ordinal);

        public IThemeTokenOverrideContributionBuilder Token(string key, object value) { _tokens[key] = value; return this; }

        public ThemeTokenOverrideDescriptor Build() => new(id, ownerPluginId, _tokens);
    }

    private sealed record InferredContributionScope(string? ActivityScopeId, bool IsGlobal);
}

#pragma warning restore CS9124
