namespace Mewoo.Workbench;

public sealed class WorkbenchState
{
    public const double SidebarMinWidth = 180;
    public const double SidebarMaxWidth = 420;
    public const double PanelMinHeight = 120;

    private double _sidebarWidth = 260;
    private double _panelHeight = 260;
    private readonly List<string> _openMainViewIds = [];

    public event Action? Changed;

    public bool SidebarCollapsed { get; private set; }

    public bool PanelVisible { get; private set; }

    public string? ActiveActivityId { get; private set; }

    public string? ActiveMainViewId { get; private set; }

    public IReadOnlyList<string> OpenMainViewIds => _openMainViewIds;

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
        Changed?.Invoke();
    }

    public void OpenMainView(string mainViewId)
    {
        if (!_openMainViewIds.Any(id => string.Equals(id, mainViewId, StringComparison.Ordinal)))
        {
            _openMainViewIds.Add(mainViewId);
        }

        if (ActiveMainViewId != mainViewId)
        {
            ActiveMainViewId = mainViewId;
        }

        Changed?.Invoke();
    }

    public void RemoveMainView(string mainViewId)
    {
        if (!_openMainViewIds.Remove(mainViewId))
        {
            return;
        }

        if (ActiveMainViewId == mainViewId)
        {
            ActiveMainViewId = _openMainViewIds.LastOrDefault();
        }

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
            _openMainViewIds.ToArray(),
            themeId,
            isAlwaysOnTop);
    }

    public void Restore(WorkbenchStateSnapshot snapshot, double windowHeight)
    {
        SidebarCollapsed = snapshot.SidebarCollapsed;
        SidebarWidth = snapshot.SidebarWidth;
        PanelVisible = snapshot.PanelVisible;
        PanelHeight = Math.Clamp(snapshot.PanelHeight, PanelMinHeight, Math.Max(PanelMinHeight, windowHeight * 0.5));
        ActiveActivityId = snapshot.ActiveActivityId;
        ActiveMainViewId = snapshot.ActiveMainViewId;
        _openMainViewIds.Clear();
        _openMainViewIds.AddRange(snapshot.OpenMainViewIds.Distinct(StringComparer.Ordinal));
        Changed?.Invoke();
    }
}
