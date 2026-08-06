using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.MewDock;
using System.Reflection;
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
        docking.TabMenuOpening += (_, args) =>
        {
            if (_workbench.CanRevealDocument(args.Pane.Component))
            {
                args.Menu.Item("在侧边栏定位", () => _workbench.RevealDocument(args.Pane.Component!));
            }
        };
        _workbench.PresentationChanged += ApplyChromeVisibility;
        _workbench.PresentationChanged += () => layoutStore.SavePresentation(
            new WorkbenchPresentationState(_workbench.ActiveActivityId, _workbench.IsSideBarVisible));
        var shell = BuildShell(docking);
        ApplyChromeVisibility();
        SoftenDockFocusBorders(docking);
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

        // 先确保目标 pane 存在,再关闭其他 view 的 pane:保证 Left border 始终至少保留一个 tab,
        // border 不会随 Close 销毁——手动调整的侧边栏宽度得以保留。
        ApplyToolPane(side.Id, side.Title, side.Content, DockEdge.Left, WorkbenchZone.SideBar, true);

        foreach (var view in _workbench.SideBarModel.Views.Where(view => view.Id != side.Id))
        {
            _docking!.Panes.FirstOrDefault(pane => pane.Component == view.Id)?.Close();
        }
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
            // pane 已存在时直接激活,不 Close+Add 重建:重建会新建 border,丢失手动调整的宽度。
            // 布局持久化后 pane 存在却隐藏边框的情形,Activate 同样使其恢复可见。
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

    private static readonly MethodInfo DefineStyleRule = typeof(StyleSheet)
        .GetMethods()
        .FirstOrDefault(m => m.Name == nameof(StyleSheet.Define)
            && m.IsGenericMethodDefinition
            && m.GetParameters() is [{ ParameterType: var p }] && p == typeof(Style))
        ?? throw new MissingMethodException(nameof(StyleSheet), nameof(StyleSheet.Define));

    /// <summary>编辑器区 tabset:默认边框 + Focused 时改为 30% 强调色混合(原 75%)。</summary>
    private static Style CreateSoftFocusTabSetStyle(Type type) => new(type)
    {
        Transitions = [Transition.Create(Control.BorderBrushProperty, 200, t => t)],
        Setters =
        [
            Setter.Create(Control.BackgroundProperty, t => t.Palette.ContainerBackground),
            Setter.Create(Control.BorderBrushProperty, t => t.Palette.ControlBorder),
            Setter.Create(Control.CornerRadiusProperty, t => t.Metrics.ControlCornerRadius),
            Setter.Create(Control.BorderThicknessProperty, t => t.Metrics.ControlBorderThickness),
        ],
        Triggers = [SoftFocusTrigger()],
    };

    /// <summary>侧边栏(ExtendedBorderBar):默认边框 + Focused 时改为 30% 强调色混合(原 75%)。</summary>
    private static Style CreateSoftFocusBorderBarStyle(Type type) => new(type)
    {
        Transitions = [Transition.Create(Control.BorderBrushProperty, 200, t => t)],
        Setters = [Setter.Create(Control.BorderBrushProperty, t => t.Palette.ControlBorder)],
        Triggers = [SoftFocusTrigger()],
    };

    private static StateTrigger SoftFocusTrigger() => new()
    {
        Match = VisualStateFlags.Focused,
        Setters =
        [
            Setter.Create(Control.BorderBrushProperty, t => t.Palette.ControlBorder.Lerp(t.Palette.Accent, 0.3)),
        ],
    };

    /// <summary>
    /// MewDock 内置 DockStyles 把焦点(tabset / 侧边栏)边框画成 ControlBorder→Accent 75% 混合,过于醒目。
    /// FlexLayoutView 的 StyleSheet 按类型注册 rule 且 GetByType 从后往前匹配——向其中追加覆盖 rule 即可
    /// 弱化焦点边框(与 NativeChromeWindow 的 30% 混合保持一致)。
    /// 目标控件类型在 MewDock 中是 internal,无法静态引用,故经反射按名解析类型。
    /// </summary>
    private static void SoftenDockFocusBorders(DockingManager docking)
    {
        if (docking.Children.FirstOrDefault() is not FrameworkElement { StyleSheet: { } sheet })
        {
            return;
        }

        var assembly = typeof(DockingManager).Assembly;
        OverrideStyle(assembly, sheet, "Aprillz.MewUI.MewDock.Controls.FlexTabSetView", CreateSoftFocusTabSetStyle);
        OverrideStyle(assembly, sheet, "Aprillz.MewUI.MewDock.Extended.ExtendedBorderBar", CreateSoftFocusBorderBarStyle);
    }

    private static void OverrideStyle(Assembly assembly, StyleSheet sheet, string typeName, Func<Type, Style> factory)
    {
        if (assembly.GetType(typeName) is not { } type)
        {
            return;
        }

        DefineStyleRule.MakeGenericMethod(type).Invoke(sheet, [factory(type)]);
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
