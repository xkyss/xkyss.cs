using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.MewDock;
using System.Text.Json;

namespace Mew.Workbench;

internal sealed class WorkbenchView
{
    private readonly Workbench _workbench;
    private DockingManager? _docking;
    private UIElement? _activityBar;
    private UIElement? _statusBar;
    private DockPane? _sideBarPane;
    private DockPane? _panelPane;
    private (string Title, UIElement Content, DockEdge Edge, string Id)? _sideBarInfo;
    private (string Title, UIElement Content, DockEdge Edge, string Id)? _panelInfo;

    internal WorkbenchView(Workbench workbench) => _workbench = workbench;

    internal DockingManager Docking => _docking ?? throw new InvalidOperationException("Build() 尚未调用");

    internal UIElement Build()
    {
        var docking = new DockingManager();
        _docking = docking;
        var theme = _workbench.ThemeContext;
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

        docking.Changed += (_, _) => layoutStore.Save(docking.SaveLayout());
        _workbench.ChromeChanged += ApplyChromeVisibility;
        var shell = BuildShell(docking);
        ApplyChromeVisibility();
        return shell;
    }

    /// <summary>应用外壳区域显隐:活动栏/状态栏直接控制;侧边栏/底部面板用 Close/重建 tool pane。</summary>
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

        ApplyToolPane(ref _sideBarPane, _sideBarInfo, _workbench.IsSideBarVisible);
        ApplyToolPane(ref _panelPane, _panelInfo, _workbench.IsPanelVisible);
    }

    private void ApplyToolPane(
        ref DockPane? pane,
        (string Title, UIElement Content, DockEdge Edge, string Id)? info,
        bool visible)
    {
        if (visible)
        {
            if (pane is null && info is { } i)
            {
                pane = _docking!.AddToolPane(i.Title, i.Content, i.Edge, i.Id);
            }
        }
        else
        {
            pane?.Close();
            pane = null;
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
            var pane = docking.AddToolPane(view.Title, ThemedPane(view.Content, theme, WorkbenchZone.SideBar), DockEdge.Left, view.Id);
            _sideBarPane ??= pane;
            _sideBarInfo ??= (view.Title, pane.Content!, DockEdge.Left, view.Id);
        }

        foreach (var document in _workbench.EditorAreaModel.Documents)
        {
            docking.AddDocumentPane(document.Title, ThemedPane(document.Content, theme, WorkbenchZone.EditorArea), document.Id);
        }

        foreach (var view in _workbench.PanelModel.Views)
        {
            var pane = docking.AddToolPane(view.Title, ThemedPane(view.Content, theme, WorkbenchZone.Panel), DockEdge.Bottom, view.Id);
            _panelPane ??= pane;
            _panelInfo ??= (view.Title, pane.Content!, DockEdge.Bottom, view.Id);
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
                        lastButton.Row(1)
                    )
            );
    }

    private static Button BuildItemButton(ActivityBarItem item, WorkbenchThemeContext theme)
    {
        var button = new Button()
            .Size(36, 36)
            .Content(item.CustomGlyph is { } custom
                ? custom
                : new GlyphElement()
                    .Kind(item.Glyph)
                    .GlyphSize(18)
                    .WithTheme((_, glyph) => glyph.Foreground(theme.ActivityBar.Foreground)))
            .ToolTip(item.Title);

        if (item.OnClick is { } onClick)
        {
            button.OnClick(onClick);
        }

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
