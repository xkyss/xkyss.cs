using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.MewDock;
using System.Text.Json;

namespace Mew.Workbench;

internal sealed class WorkbenchView
{
    private readonly Workbench _workbench;
    private DockingManager? _docking;
    private WorkbenchThemeContext? _theme;
    private UIElement? _activityBar;
    private UIElement? _statusBar;
    private readonly Dictionary<string, Button> _activityButtons = [];
    private bool _panelPinned = true; // 底部面板 Pin/Unpin 状态跟踪(初始假设固定显示)

    internal WorkbenchView(Workbench workbench) => _workbench = workbench;

    internal DockingManager Docking => _docking ?? throw new InvalidOperationException("Build() 尚未调用");

    internal UIElement Build()
    {
        var docking = new DockingManager();
        _docking = docking;
        var theme = _workbench.ThemeContext;
        _theme = theme;
        var layoutStore = new WorkbenchLayoutStore();

        docking.WithContentFactory(pane => ResolvePaneContent(pane, theme));

        if (layoutStore.TryLoad() is { } savedLayout)
        {
            try
            {
                docking.LoadLayout(savedLayout);
            }
            catch (Exception ex) when (ex is JsonException or InvalidOperationException or ArgumentException)
            {
                AddDefaultPanes(docking, theme);
            }
        }
        else
        {
            AddDefaultPanes(docking, theme);
        }

        if (layoutStore.TryLoadPresentation() is { } presentation)
        {
            _workbench.RestorePresentation(presentation);
        }

        docking.Changed += (_, _) => layoutStore.Save(docking.SaveLayout());
        _workbench.PresentationChanged += ApplyChromeVisibility;
        _workbench.PresentationChanged += () => layoutStore.SavePresentation(
            new WorkbenchPresentationState(_workbench.ActiveActivityId, _workbench.IsSideBarVisible));
        var shell = BuildShell(docking);
        ApplyChromeVisibility();
        return shell;
    }

    /// <summary>应用外壳区域显隐:活动栏/状态栏直接控制;侧边栏/底部面板按 id 查找并 Close/重建 tool pane。</summary>
    private void ApplyChromeVisibility()
    {
        if (_activityBar is not null)
        {
            _activityBar.IsVisible = _workbench.IsActivityBarVisible;
        }

        if (_statusBar is not null)
        {
            _statusBar.IsVisible = _workbench.IsStatusBarVisible;
        }

        ApplySideBarVisibility();
        ApplyActivitySelection();

        var panel = _workbench.PanelModel.Views.FirstOrDefault();
        if (panel is not null)
        {
            ApplyPanelPin(panel.Id, _workbench.IsPanelVisible);
        }
    }

    private void ApplySideBarVisibility()
    {
        if (!_workbench.IsSideBarVisible)
        {
            foreach (var view in _workbench.SideBarModel.Views)
            {
                _docking!.Panes.FirstOrDefault(pane => pane.Component == view.Id)?.Close();
            }

            return;
        }

        if (_workbench.ActiveSideBarView is not { } side)
        {
            return;
        }

        foreach (var view in _workbench.SideBarModel.Views.Where(view => view.Id != side.Id))
        {
            _docking!.Panes.FirstOrDefault(pane => pane.Component == view.Id)?.Close();
        }

        ApplyToolPane(side.Id, side.Title, side.Content, DockEdge.Left, WorkbenchZone.SideBar, true);
    }

    private void ApplyActivitySelection()
    {
        foreach (var (id, button) in _activityButtons)
        {
            button.Background(id == _workbench.ActiveActivityId
                ? _theme!.ActivityBar.Accent
                : _theme!.ActivityBar.Background);
        }
    }

    /// <summary>底部面板显隐:与 MewDock 的 Auto Hide 行为一致——隐藏 = Unpin(收起成边缘条,悬停滑出),显示 = Pin(固定)。</summary>
    private void ApplyPanelPin(string id, bool visible)
    {
        var pane = _docking!.Panes.FirstOrDefault(p => p.Component == id);
        if (pane is null || _panelPinned == visible)
        {
            return;
        }

        if (visible)
        {
            pane.Pin();
        }
        else
        {
            pane.Unpin();
        }

        _panelPinned = visible;
    }

    /// <summary>显示时按 id 查找(布局持久化路径下 pane 由 factory 创建,不依赖 AddDefaultPanes 缓存);隐藏时 Close。</summary>
    private void ApplyToolPane(string id, string title, UIElement content, DockEdge edge, WorkbenchZone zone, bool visible)
    {
        var pane = _docking!.Panes.FirstOrDefault(p => p.Component == id);
        if (visible)
        {
            if (pane is null)
            {
                pane = _docking.AddToolPane(title, ThemedPane(content, _theme!, zone), edge, id);
            }

            pane.Activate();
        }
        else
        {
            pane?.Close();
        }
    }

    private UIElement BuildShell(DockingManager docking)
    {
        _activityBar = BuildActivityBar();
        _statusBar = BuildStatusBar();
        return new Grid()
            .Rows("*,Auto")
            .Columns("Auto,*")
            .Children(
                _activityBar.Row(0).Column(0),
                docking.Row(0).Column(1),
                _statusBar.Row(1).Column(0).ColumnSpan(2)
            );
    }

    private void AddDefaultPanes(DockingManager docking, WorkbenchThemeContext theme)
    {
        foreach (var view in _workbench.SideBarModel.Views)
        {
            docking.AddToolPane(view.Title, ThemedPane(view.Content, theme, WorkbenchZone.SideBar), DockEdge.Left, view.Id);
        }

        foreach (var document in _workbench.EditorAreaModel.Documents)
        {
            docking.AddDocumentPane(document.Title, ThemedPane(document.Content, theme, WorkbenchZone.EditorArea), document.Id);
        }

        foreach (var view in _workbench.PanelModel.Views)
        {
            docking.AddToolPane(view.Title, ThemedPane(view.Content, theme, WorkbenchZone.Panel), DockEdge.Bottom, view.Id);
        }
    }

    private UIElement? ResolvePaneContent(DockPane pane, WorkbenchThemeContext theme)
    {
        if (pane.Component is not { } id)
        {
            return null;
        }

        foreach (var view in _workbench.SideBarModel.Views)
        {
            if (view.Id == id)
            {
                return ThemedPane(view.Content, theme, WorkbenchZone.SideBar);
            }
        }

        foreach (var document in _workbench.EditorAreaModel.Documents)
        {
            if (document.Id == id)
            {
                return ThemedPane(document.Content, theme, WorkbenchZone.EditorArea);
            }
        }

        foreach (var view in _workbench.PanelModel.Views)
        {
            if (view.Id == id)
            {
                return ThemedPane(view.Content, theme, WorkbenchZone.Panel);
            }
        }

        return null;
    }

    private static UIElement ThemedPane(UIElement content, WorkbenchThemeContext theme, WorkbenchZone zone)
    {
        return new Border()
            .Child(content)
            .StretchHorizontal()
            .StretchVertical()
            .WithTheme((_, border) => border.Background(theme.Get(zone).Background));
    }

    private UIElement BuildActivityBar()
    {
        var theme = _workbench.ThemeContext;
        var items = _workbench.ActivityBarModel.Items;

        if (items.Count == 0)
        {
            return new Border()
                .WithTheme((_, border) => border.Background(theme.ActivityBar.Background))
                .Child(new StackPanel().Width(48));
        }

        // VSCode 约定:最后一项(设置/管理类)钉在活动栏底部,其余图标从顶部排列。
        var lastButton = BuildItemButton(items[^1], theme);
        if (items.Count == 1)
        {
            return new Border()
                .WithTheme((_, border) => border.Background(theme.ActivityBar.Background))
                .Child(
                    new StackPanel()
                        .Width(48)
                        .Padding(6, 8)
                        .Spacing(4)
                        .Children([lastButton])
                );
        }

        var mainButtons = items.Take(items.Count - 1).Select(item => BuildItemButton(item, theme)).ToArray();
        return new Border()
            .WithTheme((_, border) => border.Background(theme.ActivityBar.Background))
            .Child(
                new Grid()
                    .Rows("*,Auto")
                    .Children(
                        new StackPanel()
                            .Width(48)
                            .Padding(6, 8)
                            .Spacing(4)
                            .Children(mainButtons)
                            .Row(0),
                        // 底部按钮与顶部同宽同留白,保证视觉对齐
                        new StackPanel()
                            .Width(48)
                            .Padding(6, 8)
                            .Children(lastButton)
                            .Row(1)
                    )
            );
    }

    private Button BuildItemButton(ActivityBarItem item, WorkbenchThemeContext theme)
    {
        var button = new Button()
            .Size(36, 36)
            .Padding(0) // 清零默认内边距,避免自定义图标(如 ⚙)被内容区裁切
            .Content(item.CustomGlyph is { } custom
                ? custom
                : new GlyphElement()
                    .Kind(item.Glyph)
                    .GlyphSize(18)
                    .WithTheme((_, glyph) => glyph.Foreground(theme.ActivityBar.Foreground)))
            .ToolTip(item.Title);

        button.OnClick(() => _workbench.SelectActivity(item.Id));
        _activityButtons.Add(item.Id, button);

        return button;
    }

    private UIElement BuildStatusBar()
    {
        var theme = _workbench.ThemeContext;
        var items = _workbench.StatusBarModel.Items;
        var children = new Element[items.Count + 1];

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            children[i] = new Label()
                .BindText(item.Text)
                .FontSize(12)
                .WithTheme((_, label) => label.Foreground(theme.StatusBar.Foreground));
        }

        children[items.Count] = new Button()
            .FontSize(12)
            .Padding(8, 4)
            .OnClick(() => CycleTheme(theme))
            .WithTheme((_, button) =>
            {
                button.Content(ThemeModeLabel(theme.Mode));
                button.Foreground(theme.StatusBar.Foreground);
                button.Background(theme.StatusBar.Background);
            });

        return new Border()
            .WithTheme((_, border) => border.Background(theme.StatusBar.Background))
            .Child(
                new StackPanel()
                    .Padding(10, 6)
                    .Spacing(16)
                    .Children(children)
            );
    }

    private static void CycleTheme(WorkbenchThemeContext theme)
    {
        var next = theme.Mode switch
        {
            ThemeVariant.System => ThemeVariant.Light,
            ThemeVariant.Light => ThemeVariant.Dark,
            _ => ThemeVariant.System,
        };

        theme.SetMode(next);
    }

    private static string ThemeModeLabel(ThemeVariant mode) => mode switch
    {
        ThemeVariant.Light => "主题: 亮色",
        ThemeVariant.Dark => "主题: 暗色",
        _ => "主题: 系统",
    };
}
