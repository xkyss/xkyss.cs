namespace Mewoo.Workbench;

public sealed record WorkbenchStateSnapshot(
    bool SidebarCollapsed,
    double SidebarWidth,
    bool PanelVisible,
    double PanelHeight,
    string? ActiveActivityId,
    string? ActiveMainViewId,
    IReadOnlyList<string> OpenMainViewIds,
    string ThemeId,
    bool IsAlwaysOnTop);

