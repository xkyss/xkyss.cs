namespace Mewoo.Workbench;

public sealed record ActivityMainViewStateSnapshot(
    string ActivityId,
    string? ActiveMainViewId,
    IReadOnlyList<string> OpenMainViewIds);

public sealed record ActivityPanelStateSnapshot(
    string ActivityId,
    bool PanelVisible,
    double PanelHeight,
    string? ActivePanelTabId,
    IReadOnlyList<string> OpenPanelTabIds);

public sealed record WorkbenchStateSnapshot(
    bool SidebarCollapsed,
    double SidebarWidth,
    bool PanelVisible,
    double PanelHeight,
    string? ActiveActivityId,
    string? ActiveMainViewId,
    IReadOnlyList<string> OpenMainViewIds,
    string ThemeId,
    bool IsAlwaysOnTop,
    IReadOnlyList<ActivityMainViewStateSnapshot>? ActivityMainViews = null,
    IReadOnlyList<ActivityPanelStateSnapshot>? ActivityPanels = null);
