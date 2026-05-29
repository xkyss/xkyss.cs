namespace Mewoo.Workbench;

public sealed class WorkbenchState
{
    public const string LogsPanelTabId = "workbench.logs";
    public const double ActivityBarWidth = 42;
    public const double MainAreaMinWidth = 560;
    public const double SidebarDefaultWidth = 220;
    public const double SidebarMinWidth = 180;
    public const double SidebarMaxWidth = 420;
    public const double SidebarCollapseWidth = 96;
    public const double PanelMinHeight = 120;
    public const double PanelDefaultHeight = 260;

    private double _sidebarWidth = SidebarDefaultWidth;
    private readonly Dictionary<string, ActivityMainViewState> _mainViewsByActivity = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ActivityPanelState> _panelsByActivity = new(StringComparer.Ordinal);
    private readonly ActivityPanelState _fallbackPanelState = new();

    public event Action? Changed;

    public bool SidebarCollapsed { get; private set; }

    public bool PanelVisible => CurrentPanelState.PanelVisible;

    public string? ActiveActivityId { get; private set; }

    public string? ActiveMainViewId { get; private set; }

    public IReadOnlyList<string> OpenMainViewIds =>
        ActiveActivityId is not null && _mainViewsByActivity.TryGetValue(ActiveActivityId, out var state)
            ? state.OpenMainViewIds
            : [];

    public IReadOnlyDictionary<string, ActivityMainViewState> ActivityMainViews => _mainViewsByActivity;

    public string? ActivePanelTabId => CurrentPanelState.ActivePanelTabId;

    public IReadOnlyList<string> OpenPanelTabIds => CurrentPanelState.OpenPanelTabIds;

    public IReadOnlyDictionary<string, ActivityPanelState> ActivityPanels => _panelsByActivity;

    public double SidebarWidth
    {
        get => _sidebarWidth;
        private set => _sidebarWidth = Math.Clamp(value, SidebarMinWidth, SidebarMaxWidth);
    }

    public double PanelHeight => CurrentPanelState.PanelHeight;

    public void ToggleSidebar()
    {
        SidebarCollapsed = !SidebarCollapsed;
        Changed?.Invoke();
    }

    public void TogglePanel()
    {
        var state = GetActivePanelState();
        state.PanelVisible = !state.PanelVisible;
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
        var state = GetActivePanelState();
        var old = state.PanelHeight;
        state.PanelHeight = ClampPanelHeight(height, windowHeight);
        if (Math.Abs(old - state.PanelHeight) > 0.1)
        {
            Changed?.Invoke();
        }
    }

    public void OpenPanelTab(string panelTabId)
    {
        if (string.IsNullOrWhiteSpace(panelTabId))
        {
            throw new ArgumentException("Panel tab id is required.", nameof(panelTabId));
        }

        var state = GetActivePanelState();
        if (!state.OpenPanelTabIds.Any(id => string.Equals(id, panelTabId, StringComparison.Ordinal)))
        {
            state.MutableOpenPanelTabIds.Add(panelTabId);
        }

        state.ActivePanelTabId = panelTabId;
        state.PanelVisible = true;
        Changed?.Invoke();
    }

    public void RemovePanelTabsExcept(IReadOnlySet<string> availablePanelTabIds)
    {
        var changed = false;
        foreach (var (activityId, state) in _panelsByActivity.ToArray())
        {
            for (var index = state.OpenPanelTabIds.Count - 1; index >= 0; index--)
            {
                if (availablePanelTabIds.Contains(state.OpenPanelTabIds[index]))
                {
                    continue;
                }

                state.MutableOpenPanelTabIds.RemoveAt(index);
                changed = true;
            }

            if (state.ActivePanelTabId is not null && !availablePanelTabIds.Contains(state.ActivePanelTabId))
            {
                state.ActivePanelTabId = state.OpenPanelTabIds.LastOrDefault();
                changed = true;
            }

            if (state.OpenPanelTabIds.Count == 0
                && !state.PanelVisible
                && Math.Abs(state.PanelHeight - PanelDefaultHeight) < 0.1)
            {
                _panelsByActivity.Remove(activityId);
            }
        }

        if (changed)
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
                .ToArray(),
            _panelsByActivity
                .Select(pair => new ActivityPanelStateSnapshot(
                    pair.Key,
                    pair.Value.PanelVisible,
                    pair.Value.PanelHeight,
                    pair.Value.ActivePanelTabId,
                    pair.Value.OpenPanelTabIds.ToArray()))
                .ToArray());
    }

    public void Restore(WorkbenchStateSnapshot snapshot, double windowHeight)
    {
        SidebarCollapsed = snapshot.SidebarCollapsed;
        SidebarWidth = snapshot.SidebarWidth;
        ActiveActivityId = snapshot.ActiveActivityId;
        _mainViewsByActivity.Clear();
        _panelsByActivity.Clear();

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

        if (snapshot.ActivityPanels is { Count: > 0 })
        {
            foreach (var activityPanelState in snapshot.ActivityPanels)
            {
                if (string.IsNullOrWhiteSpace(activityPanelState.ActivityId))
                {
                    continue;
                }

                var state = GetOrCreatePanelState(activityPanelState.ActivityId);
                state.PanelVisible = activityPanelState.PanelVisible;
                state.PanelHeight = ClampPanelHeight(activityPanelState.PanelHeight, windowHeight);
                state.MutableOpenPanelTabIds.AddRange(activityPanelState.OpenPanelTabIds.Distinct(StringComparer.Ordinal));
                state.ActivePanelTabId = state.OpenPanelTabIds.Contains(activityPanelState.ActivePanelTabId, StringComparer.Ordinal)
                    ? activityPanelState.ActivePanelTabId
                    : state.OpenPanelTabIds.LastOrDefault();
            }
        }
        else if (snapshot.ActiveActivityId is not null)
        {
            var state = GetOrCreatePanelState(snapshot.ActiveActivityId);
            state.PanelVisible = snapshot.PanelVisible;
            state.PanelHeight = ClampPanelHeight(snapshot.PanelHeight, windowHeight);
            if (state.PanelVisible)
            {
                state.MutableOpenPanelTabIds.Add(LogsPanelTabId);
                state.ActivePanelTabId = LogsPanelTabId;
            }
        }

        Changed?.Invoke();
    }

    private ActivityPanelState CurrentPanelState =>
        ActiveActivityId is not null && _panelsByActivity.TryGetValue(ActiveActivityId, out var state)
            ? state
            : _fallbackPanelState;

    private ActivityPanelState GetActivePanelState()
    {
        if (ActiveActivityId is null)
        {
            return _fallbackPanelState;
        }

        return GetOrCreatePanelState(ActiveActivityId);
    }

    private ActivityPanelState GetOrCreatePanelState(string activityId)
    {
        if (!_panelsByActivity.TryGetValue(activityId, out var state))
        {
            state = new ActivityPanelState();
            _panelsByActivity[activityId] = state;
        }

        return state;
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

    private static double ClampPanelHeight(double height, double windowHeight)
    {
        return Math.Clamp(height, PanelMinHeight, Math.Max(PanelMinHeight, windowHeight * 0.5));
    }
}

public sealed class ActivityMainViewState
{
    private readonly List<string> _openMainViewIds = [];

    public string? ActiveMainViewId { get; internal set; }

    public IReadOnlyList<string> OpenMainViewIds => _openMainViewIds;

    internal List<string> MutableOpenMainViewIds => _openMainViewIds;
}

public sealed class ActivityPanelState
{
    private readonly List<string> _openPanelTabIds = [];

    public bool PanelVisible { get; internal set; }

    public double PanelHeight { get; internal set; } = WorkbenchState.PanelDefaultHeight;

    public string? ActivePanelTabId { get; internal set; }

    public IReadOnlyList<string> OpenPanelTabIds => _openPanelTabIds;

    internal List<string> MutableOpenPanelTabIds => _openPanelTabIds;
}
