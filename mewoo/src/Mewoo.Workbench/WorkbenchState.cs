namespace Mewoo.Workbench;

public sealed class WorkbenchState
{
    public const double SidebarMinWidth = 180;
    public const double SidebarMaxWidth = 420;
    public const double PanelMinHeight = 120;

    private double _sidebarWidth = 260;
    private double _panelHeight = 260;

    public event Action? Changed;

    public bool SidebarCollapsed { get; private set; }

    public bool PanelVisible { get; private set; }

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
}

