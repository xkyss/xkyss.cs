using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;

namespace Mew.Workbench;

/// <summary>
/// 自绘标题栏窗口壳:基于 DWM 原生帧扩展(ExtendClientAreaTitleBarHeight)替换系统标题栏,
/// 保留 OS 圆角、阴影、缩放与贴边;标题栏左/中/右三区可注入,窗口按钮原生优先、缺失时自绘兜底。
/// 实现参照 MewUI 官方样例 NativeCustomWindow(取舍见 ADR 000102-01)。
/// </summary>
public sealed class NativeChromeWindow : Window
{
    private const double TitleBarHeight = 28;
    private const double ButtonWidth = 32;
    private const double ChromeButtonSize = 4;

    private readonly Border _contentArea;
    private readonly Border _chromeBorder;
    private readonly AlphaTextPanel _titleBar;
    private readonly TextBlock _titleText;
    private readonly StackPanel _controlButtons;
    private readonly StackPanel _leftArea;
    private readonly StackPanel _rightArea;
    private readonly Button _minimizeBtn;
    private readonly Button _maximizeBtn;
    private Theme? _theme;

    public NativeChromeWindow()
    {
        ExtendClientAreaTitleBarHeight = TitleBarHeight;
        base.Padding = new Thickness(0);

        StyleSheet = new StyleSheet();
        StyleSheet.Define("chrome", CreateChromeButtonStyle);
        StyleSheet.Define("close", CreateCloseButtonStyle);

        _titleText = new TextBlock
        {
            IsHitTestVisible = false,
            FontWeight = FontWeight.SemiBold,
            FontSize = 13,
            Margin = new Thickness(8, 0),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _titleText.SetBinding(TextBlock.TextProperty, this, TitleProperty);

        _minimizeBtn = CreateChromeButton(GlyphKind.WindowMinimize);
        _minimizeBtn.Click += () => Minimize();
        _minimizeBtn.SetBinding(UIElement.IsVisibleProperty, this, CanMinimizeProperty);

        var maxGlyph = new GlyphElement().Kind(GlyphKind.WindowMaximize).GlyphSize(ChromeButtonSize);
        _maximizeBtn = CreateChromeButton(maxGlyph);
        _maximizeBtn.Click += () =>
        {
            if (WindowState == WindowState.Maximized)
            {
                Restore();
            }
            else
            {
                Maximize();
            }
        };
        _maximizeBtn.SetBinding(UIElement.IsVisibleProperty, this, CanMaximizeProperty);

        var closeBtn = CreateChromeButton(GlyphKind.Cross, isClose: true);
        closeBtn.Click += () => Close();
        closeBtn.SetBinding(UIElement.IsVisibleProperty, this, CanCloseProperty);

        _controlButtons = new StackPanel { Orientation = Orientation.Horizontal };
        _controlButtons.Add(_minimizeBtn);
        _controlButtons.Add(_maximizeBtn);
        _controlButtons.Add(closeBtn);

        _leftArea = new StackPanel { Orientation = Orientation.Horizontal };
        _rightArea = new StackPanel { Orientation = Orientation.Horizontal };

        var titleBarContent = new DockPanel().Children(
            new Border().DockRight().Child(_controlButtons),
            new Border().DockRight().Child(_rightArea),
            new Border().DockLeft().Child(_leftArea),
            _titleText
        );
        _titleBar = new AlphaTextPanel
        {
            MinHeight = TitleBarHeight,
            Content = titleBarContent,
        };
        _titleBar.SetBinding(BackgroundProperty, this, BackgroundProperty);

        _titleBar.MouseDown += e =>
        {
            if (e.Button == MouseButton.Left && e.ClickCount == 1)
            {
                DragMove();
                e.Handled = true;
            }
        };

        _titleBar.MouseDoubleClick += e =>
        {
            if (e.Button == MouseButton.Left && CanMaximize)
            {
                if (e.GetPosition(_titleBar) is Point p && (_leftArea.Bounds.Contains(p) || _rightArea.Bounds.Contains(p)))
                {
                    e.Handled = true;
                    return;
                }

                if (WindowState == WindowState.Maximized)
                {
                    Restore();
                }
                else
                {
                    Maximize();
                }
                e.Handled = true;
            }
        };

        _contentArea = new Border { Padding = new Thickness(0) };

        _chromeBorder = new Border
        {
            BorderThickness = 0,
            Child = new DockPanel().Children(
                _titleBar.DockTop(),
                _contentArea
            ),
        };
        _chromeBorder.SetBinding(Border.BorderBrushProperty, this, BorderBrushProperty);

        _contentArea.Child = new ContentPresenter();
        Template = new DelegateControlTemplate<NativeChromeWindow>((window, _) => window._chromeBorder);

        ClientSizeChanged += _ =>
        {
            OnWindowStateVisualUpdate();
            UpdateChromeButtonVisibility();
        };
        Activated += UpdateChromeAppearance;
        Deactivated += UpdateChromeAppearance;
        Loaded += OnLoaded;
    }

    private void OnAppThemeChanged(Theme oldTheme, Theme newTheme)
    {
        _theme = newTheme;
        UpdateChromeAppearance();

        // 0.19.1 对窗口模板(chrome)内容的主题重绘存在缺陷:运行时切主题后图标/菜单/标题可能不再重绘,
        // 强制整窗失效以触发重渲染。
        Invalidate();
        InvalidateVisual();
        InvalidateMeasure();
        InvalidateArrange();
        PerformLayout();
    }

    /// <summary>标题栏左区注入点(如图标、菜单栏)。</summary>
    public StackPanel TitleBarLeft => _leftArea;

    /// <summary>标题栏右区注入点(如设置入口)。</summary>
    public StackPanel TitleBarRight => _rightArea;

    /// <summary>内容区边距;窗口级 Padding 重定向到内容区,标题栏不参与。</summary>
    public new Thickness Padding
    {
        get => _contentArea.Padding;
        set => _contentArea.Padding = value;
    }

    private static Style CreateChromeButtonStyle()
        => new(typeof(Button))
        {
            Transitions = [Transition.Create(Control.BackgroundProperty, 120, t => t)],
            Setters =
            [
                Setter.Create(Control.BackgroundProperty, t => t.Palette.ButtonFace.WithAlpha(0)),
                Setter.Create(Control.BorderThicknessProperty, 0.0),
                Setter.Create(Control.CornerRadiusProperty, 0.0),
                Setter.Create(Control.PaddingProperty, new Thickness(0)),
            ],
            Triggers =
            [
                new StateTrigger
                {
                    Match = VisualStateFlags.Hot,
                    Setters = [Setter.Create(Control.BackgroundProperty, t => t.Palette.ButtonFace)],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.Pressed,
                    Setters = [Setter.Create(Control.BackgroundProperty, t => t.Palette.ButtonPressedBackground)],
                },
            ],
        };

    private static Style CreateCloseButtonStyle()
        => new(typeof(Button))
        {
            Transitions = [Transition.Create(Control.BackgroundProperty, 120, t => t)],
            Setters =
            [
                Setter.Create(Control.BackgroundProperty, Color.FromRgb(232, 17, 35).WithAlpha(0)),
                Setter.Create(Control.BorderThicknessProperty, 0.0),
                Setter.Create(Control.CornerRadiusProperty, 0.0),
                Setter.Create(Control.PaddingProperty, new Thickness(0)),
            ],
            Triggers =
            [
                new StateTrigger
                {
                    Match = VisualStateFlags.Hot,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, Color.FromRgb(232, 17, 35)),
                        // 0.19.1 中 ForegroundProperty 声明在 TextElement(Button 的基类链上),
                        // 而非样例 main 分支的 Control —— 勿改回 Control.ForegroundProperty(无法编译)。
                        Setter.Create(TextElement.ForegroundProperty, Color.White),
                    ],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.Pressed,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, Color.FromRgb(200, 12, 28)),
                        Setter.Create(TextElement.ForegroundProperty, Color.White),
                    ],
                },
            ],
        };

    private void OnLoaded()
    {
        if (BorderBrush.A > 0
            && !ChromeCapabilities.HasFlag(WindowChromeCapabilities.NativeBorderColor)
            && !ChromeCapabilities.HasFlag(WindowChromeCapabilities.NativeWindowBorder))
        {
            _chromeBorder.BorderThickness = 1;
        }

        // 0.19.1 中 Window 的 OnThemeChanged/ThemeChanged 不随应用主题切换触发(样例 main 分支行为不同),
        // 须监听 Application.ThemeChanged 才能让 chrome 配色跟随亮/暗切换。窗口可能在 Application.Run 之前创建,
        // 故在 Loaded(应用已运行)时补订阅。
        if (Application.IsRunning && Application.Current is { } app)
        {
            app.ThemeChanged -= OnAppThemeChanged;
            app.ThemeChanged += OnAppThemeChanged;
        }
    }

    protected override void OnMewPropertyChanged(MewProperty property)
    {
        base.OnMewPropertyChanged(property);

        if (ChromeCapabilities.HasFlag(WindowChromeCapabilities.NativeBorderColor)
            && property.Name == nameof(BorderBrush))
        {
            SetWindowBorderColor(BorderBrush);
        }
    }

    private void UpdateChromeAppearance()
    {
        var palette = CurrentTheme().Palette;
        var accentBorder = IsActive ? palette.Accent : palette.ControlBorder;

        BorderBrush = accentBorder;
        _titleText.Foreground = IsActive ? palette.WindowText : palette.DisabledText;
    }

    protected override void OnThemeChanged(Theme oldTheme, Theme newTheme)
    {
        base.OnThemeChanged(oldTheme, newTheme);
        _theme = newTheme;
        UpdateChromeAppearance();
    }

    private void UpdateChromeButtonVisibility()
    {
        bool hasExtend = ChromeCapabilities.HasFlag(WindowChromeCapabilities.ExtendClientArea);
        _titleBar.IsVisible = hasExtend;
        _controlButtons.IsVisible = !HasNativeChromeButtons;
        _titleBar.Padding = NativeChromeButtonInset;
    }

    private void OnWindowStateVisualUpdate()
    {
        if (_maximizeBtn.Content is GlyphElement glyph)
        {
            glyph.Kind = WindowState == WindowState.Maximized ? GlyphKind.WindowRestore : GlyphKind.WindowMaximize;
        }
    }

    private Theme CurrentTheme() =>
        _theme
        ?? (Application.IsRunning && Application.Current is { } app ? app.Theme : WorkbenchThemeContext.CreateFallbackTheme());

    private static Button CreateChromeButton(GlyphKind kind, bool isClose = false)
    {
        var glyph = new GlyphElement().Kind(kind).GlyphSize(ChromeButtonSize);
        return CreateChromeButton(glyph, isClose);
    }

    private static Button CreateChromeButton(Element content, bool isClose = false)
        => new()
        {
            Content = content,
            MinWidth = ButtonWidth,
            MinHeight = TitleBarHeight,
            StyleName = isClose ? "close" : "chrome",
        };

    /// <summary>
    /// 在 DWM 扩展标题栏区域启用 alpha 正确的文字渲染(GDI 后端需要)。
    /// </summary>
    internal sealed class AlphaTextPanel : ContentControl
    {
        protected override void RenderSubtree(IGraphicsContext context)
        {
            context.EnableAlphaTextHint = true;
            try
            {
                base.RenderSubtree(context);
            }
            finally
            {
                context.EnableAlphaTextHint = false;
            }
        }
    }
}
