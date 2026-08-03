using Aprillz.MewUI.Controls;

namespace Mew.Workbench;

/// <summary>
/// Compile-time, type-safe builder for a five-zone MewUI workbench.
/// </summary>
public sealed class Workbench
{
    private readonly ActivityBar _activityBar = new();
    private readonly SideBar _sideBar = new();
    private readonly EditorArea _editorArea = new();
    private readonly BottomPanel _panel = new();
    private readonly StatusBar _statusBar = new();
    private readonly WorkbenchThemeContext _theme = new();

    public WorkbenchThemeContext ThemeContext => _theme;

    public Workbench Theme(Action<WorkbenchThemeContext> configure)
    {
        configure(_theme);
        return this;
    }

    public Workbench ActivityBar(Action<ActivityBar> configure)
    {
        configure(_activityBar);
        return this;
    }

    public Workbench SideBar(Action<SideBar> configure)
    {
        configure(_sideBar);
        return this;
    }

    public Workbench EditorArea(Action<EditorArea> configure)
    {
        configure(_editorArea);
        return this;
    }

    public Workbench Panel(Action<BottomPanel> configure)
    {
        configure(_panel);
        return this;
    }

    public Workbench StatusBar(Action<StatusBar> configure)
    {
        configure(_statusBar);
        return this;
    }

    public UIElement Build() => new WorkbenchView(this).Build();

    internal ActivityBar ActivityBarModel => _activityBar;

    internal SideBar SideBarModel => _sideBar;

    internal EditorArea EditorAreaModel => _editorArea;

    internal BottomPanel PanelModel => _panel;

    internal StatusBar StatusBarModel => _statusBar;
}
