namespace Mewoo.Workbench;

public sealed class WorkbenchState
{
    public const double ActivityBarWidth = 42;
    public const double MainAreaMinWidth = 560;
    public const double SidebarDefaultWidth = 220;
    public const double SidebarMinWidth = 180;
    public const double SidebarMaxWidth = 420;
    public const double SidebarCollapseWidth = 96;
    public const double PanelMinHeight = 120;

    private double _sidebarWidth = SidebarDefaultWidth;
    private double _panelHeight = 260;
    private readonly Dictionary<string, ActivityMainViewState> _mainViewsByActivity = new(StringComparer.Ordinal);

    public event Action? Changed;

    public bool SidebarCollapsed { get; private set; }

    public bool PanelVisible { get; private set; }

    public string? ActiveActivityId { get; private set; }

    public string? ActiveMainViewId { get; private set; }

    public IReadOnlyList<string> OpenMainViewIds =>
        ActiveActivityId is not null && _mainViewsByActivity.TryGetValue(ActiveActivityId, out var state)
            ? state.OpenMainViewIds
            : [];

    public IReadOnlyDictionary<string, ActivityMainViewState> ActivityMainViews => _mainViewsByActivity;

    public double SidebarWidth
    {
        get => _sidebarWidth;
        private set => _sidebarWidth = Math.Clamp(value, SidebarMinWidth, SidebarMaxWidth);
    }

    public double PanelHeight
    {
        get => _panelHeight;
        private set => _panelHeight = Math.Max(PanelMinHeight, value);
    }

    public void ToggleSidebar()
    {
        SidebarCollapsed = !SidebarCollapsed;
        Changed?.Invoke();
    }

    public void TogglePanel()
    {
        PanelVisible = !PanelVisible;
        Changed?.Invoke();
    }

    public void SetSidebarWidth(double width)
    {
        var old = SidebarWidth;
        SidebarWidth = width;
        if (Math.Abs(old - SidebarWidth) > 0.1)
        {
            Changed?.Invoke();
        }
    }

    public void ResizeSidebar(double requestedWidth, double windowWidth)
    {
        var oldCollapsed = SidebarCollapsed;
        var oldWidth = SidebarWidth;

        if (requestedWidth <= SidebarCollapseWidth)
        {
            SidebarCollapsed = true;
        }
        else
        {
            SidebarCollapsed = false;
            SidebarWidth = Math.Min(requestedWidth, GetSidebarMaxWidth(windowWidth));
        }

        if (oldCollapsed != SidebarCollapsed || Math.Abs(oldWidth - SidebarWidth) > 0.1)
        {
            Changed?.Invoke();
        }
    }

    public void SetPanelHeight(double height, double windowHeight)
    {
        var max = Math.Max(PanelMinHeight, windowHeight * 0.5);
        var old = PanelHeight;
        PanelHeight = Math.Clamp(height, PanelMinHeight, max);
        if (Math.Abs(old - PanelHeight) > 0.1)
        {
            Changed?.Invoke();
        }
    }

    public void SetActiveActivity(string? activityId)
    {
        if (ActiveActivityId == activityId)
        {
            return;
        }

        ActiveActivityId = activityId;
        ActiveMainViewId = activityId is not null && _mainViewsByActivity.TryGetValue(activityId, out var state)
            ? state.ActiveMainViewId
            : null;
        Changed?.Invoke();
    }

    public void OpenMainView(string mainViewId, string activityId)
    {
        if (string.IsNullOrWhiteSpace(activityId))
        {
            throw new ArgumentException("Activity id is required.", nameof(activityId));
        }

        var state = GetOrCreateMainViewState(activityId);
        if (!state.OpenMainViewIds.Any(id => string.Equals(id, mainViewId, StringComparison.Ordinal)))
        {
            state.MutableOpenMainViewIds.Add(mainViewId);
        }

        if (state.ActiveMainViewId != mainViewId)
        {
            state.ActiveMainViewId = mainViewId;
        }

        ActiveActivityId = activityId;
        ActiveMainViewId = state.ActiveMainViewId;
        Changed?.Invoke();
    }

    public void RemoveMainView(string mainViewId, string? activityId = null)
    {
        var changed = false;
        if (activityId is not null)
        {
            changed = RemoveMainViewFromActivity(mainViewId, activityId);
        }
        else
        {
            foreach (var key in _mainViewsByActivity.Keys.ToArray())
            {
                changed = RemoveMainViewFromActivity(mainViewId, key) || changed;
            }
        }

        if (!changed)
        {
            return;
        }

        if (ActiveActivityId is not null && _mainViewsByActivity.TryGetValue(ActiveActivityId, out var activeState))
        {
            ActiveMainViewId = activeState.ActiveMainViewId;
        }
        else
        {
            ActiveMainViewId = null;
        }

        Changed?.Invoke();
    }

    public void RemoveMainViewsExcept(IReadOnlySet<string> availableMainViewIds)
    {
        var changed = false;
        foreach (var (activityId, state) in _mainViewsByActivity.ToArray())
        {
            for (var index = state.OpenMainViewIds.Count - 1; index >= 0; index--)
            {
                if (availableMainViewIds.Contains(state.OpenMainViewIds[index]))
                {
                    continue;
                }

                state.MutableOpenMainViewIds.RemoveAt(index);
                changed = true;
            }

            if (state.ActiveMainViewId is not null && !availableMainViewIds.Contains(state.ActiveMainViewId))
            {
                state.ActiveMainViewId = state.OpenMainViewIds.LastOrDefault();
                changed = true;
            }

            if (state.OpenMainViewIds.Count == 0)
            {
                _mainViewsByActivity.Remove(activityId);
            }
        }

        if (!changed)
        {
            return;
        }

        ActiveMainViewId = ActiveActivityId is not null && _mainViewsByActivity.TryGetValue(ActiveActivityId, out var activeState)
            ? activeState.ActiveMainViewId
            : null;
        Changed?.Invoke();
    }

    public WorkbenchStateSnapshot CreateSnapshot(string themeId, bool isAlwaysOnTop)
    {
        return new WorkbenchStateSnapshot(
            SidebarCollapsed,
            SidebarWidth,
            PanelVisible,
            PanelHeight,
            ActiveActivityId,
            ActiveMainViewId,
            OpenMainViewIds.ToArray(),
            themeId,
            isAlwaysOnTop,
            _mainViewsByActivity
                .Select(pair => new ActivityMainViewStateSnapshot(
                    pair.Key,
                    pair.Value.ActiveMainViewId,
                    pair.Value.OpenMainViewIds.ToArray()))
                .ToArray());
    }

    public void Restore(WorkbenchStateSnapshot snapshot, double windowHeight)
    {
        SidebarCollapsed = snapshot.SidebarCollapsed;
        SidebarWidth = snapshot.SidebarWidth;
        PanelVisible = snapshot.PanelVisible;
        PanelHeight = Math.Clamp(snapshot.PanelHeight, PanelMinHeight, Math.Max(PanelMinHeight, windowHeight * 0.5));
        ActiveActivityId = snapshot.ActiveActivityId;
        _mainViewsByActivity.Clear();
        if (snapshot.ActivityMainViews is { Count: > 0 })
        {
            foreach (var activityState in snapshot.ActivityMainViews)
            {
                if (string.IsNullOrWhiteSpace(activityState.ActivityId))
                {
                    continue;
                }

                var state = GetOrCreateMainViewState(activityState.ActivityId);
                state.MutableOpenMainViewIds.AddRange(activityState.OpenMainViewIds.Distinct(StringComparer.Ordinal));
                state.ActiveMainViewId = state.OpenMainViewIds.Contains(activityState.ActiveMainViewId, StringComparer.Ordinal)
                    ? activityState.ActiveMainViewId
                    : state.OpenMainViewIds.LastOrDefault();
            }
        }
        else if (snapshot.ActiveActivityId is not null)
        {
            var state = GetOrCreateMainViewState(snapshot.ActiveActivityId);
            state.MutableOpenMainViewIds.AddRange(snapshot.OpenMainViewIds.Distinct(StringComparer.Ordinal));
            state.ActiveMainViewId = state.OpenMainViewIds.Contains(snapshot.ActiveMainViewId, StringComparer.Ordinal)
                ? snapshot.ActiveMainViewId
                : state.OpenMainViewIds.LastOrDefault();
        }

        ActiveMainViewId = ActiveActivityId is not null && _mainViewsByActivity.TryGetValue(ActiveActivityId, out var activeState)
            ? activeState.ActiveMainViewId
            : null;
        Changed?.Invoke();
    }

    private ActivityMainViewState GetOrCreateMainViewState(string activityId)
    {
        if (!_mainViewsByActivity.TryGetValue(activityId, out var state))
        {
            state = new ActivityMainViewState();
            _mainViewsByActivity[activityId] = state;
        }

        return state;
    }

    private bool RemoveMainViewFromActivity(string mainViewId, string activityId)
    {
        if (!_mainViewsByActivity.TryGetValue(activityId, out var state)
            || !state.MutableOpenMainViewIds.Remove(mainViewId))
        {
            return false;
        }

        if (state.ActiveMainViewId == mainViewId)
        {
            state.ActiveMainViewId = state.OpenMainViewIds.LastOrDefault();
        }

        if (state.OpenMainViewIds.Count == 0)
        {
            _mainViewsByActivity.Remove(activityId);
        }

        return true;
    }

    private static double GetSidebarMaxWidth(double windowWidth)
    {
        if (double.IsNaN(windowWidth) || double.IsInfinity(windowWidth) || windowWidth <= 0)
        {
            return SidebarMaxWidth;
        }

        var maxByMainArea = windowWidth - ActivityBarWidth - MainAreaMinWidth;
        return Math.Clamp(maxByMainArea, SidebarMinWidth, SidebarMaxWidth);
    }
}

public sealed class ActivityMainViewState
{
    private readonly List<string> _openMainViewIds = [];

    public string? ActiveMainViewId { get; internal set; }

    public IReadOnlyList<string> OpenMainViewIds => _openMainViewIds;

    internal List<string> MutableOpenMainViewIds => _openMainViewIds;
}
