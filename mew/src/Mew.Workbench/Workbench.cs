using Aprillz.MewUI.Controls;
using Aprillz.MewUI.MewDock;

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
    private DockingManager? _docking;
    private bool _activityBarVisible = true;
    private bool _sideBarVisible = true;
    private bool _panelVisible = true;
    private bool _statusBarVisible = true;

    /// <summary>外壳区域显隐变更通知(WorkbenchView 订阅后应用 IsVisible)。</summary>
    internal event Action? ChromeChanged;

    public bool IsActivityBarVisible => _activityBarVisible;
    public bool IsSideBarVisible => _sideBarVisible;
    public bool IsPanelVisible => _panelVisible;
    public bool IsStatusBarVisible => _statusBarVisible;

    public void ToggleActivityBar() { _activityBarVisible = !_activityBarVisible; ChromeChanged?.Invoke(); }
    public void ToggleSideBar() { _sideBarVisible = !_sideBarVisible; ChromeChanged?.Invoke(); }
    public void TogglePanel() { _panelVisible = !_panelVisible; ChromeChanged?.Invoke(); }
    public void ToggleStatusBar() { _statusBarVisible = !_statusBarVisible; ChromeChanged?.Invoke(); }

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

    public UIElement Build()
    {
        var view = new WorkbenchView(this);
        var result = view.Build();
        _docking = view.Docking;
        return result;
    }

    /// <summary>
    /// 激活(必要时先创建)编辑器区指定文档标签,供运行时按需打开文档(如设置页)。
    /// </summary>
    public void OpenDocument(string id)
    {
        if (_docking is not { } docking)
        {
            return;
        }

        var pane = docking.DocumentPanes.FirstOrDefault(p => p.Component == id);
        if (pane is not null)
        {
            pane.Activate();
            return;
        }

        // 持久化布局可能不含新文档(如升级引入的设置页),按注册信息补建后激活。
        var document = _editorArea.Documents.FirstOrDefault(d => d.Id == id);
        if (document is not null)
        {
            docking.AddDocumentPane(document.Title, document.Content, document.Id).Activate();
        }
    }

    /// <summary>
    /// 激活侧边栏指定工具视图(活动栏上下文切换用,如「启动」⇄「设置」)。
    /// </summary>
    public void OpenToolPane(string id)
    {
        if (_docking is not { } docking)
        {
            return;
        }

        var pane = docking.Panes.FirstOrDefault(p => p.Component == id);
        if (pane is not null)
        {
            pane.Activate();
        }
    }

    internal ActivityBar ActivityBarModel => _activityBar;

    internal SideBar SideBarModel => _sideBar;

    internal EditorArea EditorAreaModel => _editorArea;

    internal BottomPanel PanelModel => _panel;

    internal StatusBar StatusBarModel => _statusBar;
}
