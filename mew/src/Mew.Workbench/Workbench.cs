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
    private string? _activeActivityId;

    /// <summary>工作台呈现状态变更通知(WorkbenchView 订阅后应用区域显隐与活动栏上下文)。</summary>
    internal event Action? PresentationChanged;

    public bool IsActivityBarVisible => _activityBarVisible;
    public bool IsSideBarVisible => _sideBarVisible;
    public bool IsPanelVisible => _panelVisible;
    public bool IsStatusBarVisible => _statusBarVisible;
    public string? ActiveActivityId => _activeActivityId;

    public void ToggleActivityBar() { _activityBarVisible = !_activityBarVisible; PresentationChanged?.Invoke(); }
    public void ToggleSideBar() { _sideBarVisible = !_sideBarVisible; PresentationChanged?.Invoke(); }
    public void TogglePanel() { _panelVisible = !_panelVisible; PresentationChanged?.Invoke(); }
    public void ToggleStatusBar() { _statusBarVisible = !_statusBarVisible; PresentationChanged?.Invoke(); }

    /// <summary>
    /// 选择活动栏上下文。选择当前项时切换侧边栏显隐；选择其他项时显示其唯一对应的侧边栏视图。
    /// </summary>
    public void SelectActivity(string id)
    {
        EnsureActivityContext(id);

        if (_activeActivityId == id)
        {
            _sideBarVisible = !_sideBarVisible;
        }
        else
        {
            _activeActivityId = id;
            _sideBarVisible = true;
        }

        PresentationChanged?.Invoke();
    }

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
        ValidateActivityContexts();
        _activeActivityId ??= _activityBar.Items.FirstOrDefault()?.Id;

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

    internal ActivityBar ActivityBarModel => _activityBar;

    internal SideBar SideBarModel => _sideBar;

    internal EditorArea EditorAreaModel => _editorArea;

    internal BottomPanel PanelModel => _panel;

    internal StatusBar StatusBarModel => _statusBar;

    internal SideBarView? ActiveSideBarView => _activeActivityId is null
        ? null
        : _sideBar.Views.Single(view => view.Id == _activeActivityId);

    internal void RestorePresentation(WorkbenchPresentationState state)
    {
        _activeActivityId = _activityBar.Items.Any(item => item.Id == state.ActiveActivityId)
            ? state.ActiveActivityId
            : _activityBar.Items.FirstOrDefault()?.Id;
        _sideBarVisible = state.IsSideBarVisible;
    }

    private void ValidateActivityContexts()
    {
        var activityIds = _activityBar.Items.Select(item => item.Id).ToList();
        var sideBarIds = _sideBar.Views.Select(view => view.Id).ToList();

        if (activityIds.Count != activityIds.Distinct(StringComparer.Ordinal).Count()
            || sideBarIds.Count != sideBarIds.Distinct(StringComparer.Ordinal).Count()
            || !activityIds.Order(StringComparer.Ordinal).SequenceEqual(sideBarIds.Order(StringComparer.Ordinal)))
        {
            throw new InvalidOperationException("活动栏项必须与侧边栏视图按唯一 ID 一对一配对。");
        }
    }

    private void EnsureActivityContext(string id)
    {
        if (_activityBar.Items.All(item => item.Id != id)
            || _sideBar.Views.All(view => view.Id != id))
        {
            throw new ArgumentException($"不存在活动栏上下文“{id}”。", nameof(id));
        }
    }
}
