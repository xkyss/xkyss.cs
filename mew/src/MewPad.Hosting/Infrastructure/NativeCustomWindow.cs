namespace MewPad.Hosting.Infrastructure;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;

/// <summary>
/// 无边框自定义标题栏窗口，基于 DWM 帧扩展（Win11）。
/// 圆角、阴影和 resize 由 OS 处理。
/// 参考: Resty.Gui/Infrastructure/NativeCustomWindow.cs
/// </summary>
public class NativeCustomWindow : Window
{
    private const double DefaultTitleBarHeight = 28;
    private const double ButtonWidth = 32;
    private const double ChromeButtonSize = 4;

    private readonly Border _contentArea;
    private readonly Border _chromeBorder;
    private readonly AlphaTextPanel _titleBar;
    private readonly TextBlock _titleText;
    private readonly StackPanel _controlButtons;
    protected readonly StackPanel _leftArea;
    protected readonly StackPanel _rightArea;
    private readonly Button _minimizeBtn;
    private readonly Button _maximizeBtn;

    public NativeCustomWindow()
    {
        ExtendClientAreaTitleBarHeight = DefaultTitleBarHeight;
        base.Padding = new Thickness(0);
        StyleSheet = new StyleSheet();
        StyleSheet.Define("chrome", ChromeButtonStyle);
        StyleSheet.Define("close", CloseButtonStyle);

        // 标题文本（居中，禁用鼠标命中测试）
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

        // Chrome 按钮：最小化
        _minimizeBtn = CreateChromeButton(GlyphKind.WindowMinimize);
        _minimizeBtn.Click += () => Minimize();
        _minimizeBtn.SetBinding(UIElement.IsVisibleProperty, this, CanMinimizeProperty);

        // Chrome 按钮：最大化 / 还原
        var maxGlyph = new GlyphElement().Kind(GlyphKind.WindowMaximize).GlyphSize(ChromeButtonSize);
        _maximizeBtn = CreateChromeButton(maxGlyph);
        _maximizeBtn.Click += () =>
        {
            if (WindowState == WindowState.Maximized) Restore();
            else Maximize();
        };
        _maximizeBtn.SetBinding(UIElement.IsVisibleProperty, this, CanMaximizeProperty);

        // Chrome 按钮：关闭
        var closeBtn = CreateChromeButton(GlyphKind.Cross, isClose: true);
        closeBtn.Click += () => Close();
        closeBtn.SetBinding(UIElement.IsVisibleProperty, this, CanCloseProperty);

        _controlButtons = new StackPanel { Orientation = Orientation.Horizontal };
        _controlButtons.Add(_minimizeBtn);
        _controlButtons.Add(_maximizeBtn);
        _controlButtons.Add(closeBtn);

        // 标题栏左/右扩展区域
        _leftArea = new StackPanel { Orientation = Orientation.Horizontal };
        _rightArea = new StackPanel { Orientation = Orientation.Horizontal };

        // 标题栏内容（DockPanel 布局）
        var titleBarContent = new DockPanel().Children(
            new Border().DockRight().Child(_controlButtons),
            new Border().DockRight().Child(_rightArea),
            new Border().DockLeft().Child(_leftArea),
            _titleText
        );

        _titleBar = new AlphaTextPanel
        {
            MinHeight = DefaultTitleBarHeight,
            Content = titleBarContent,
        };
        _titleBar.SetBinding(BackgroundProperty, this, BackgroundProperty);

        // 双击标题栏最大化/还原（排除左右按钮区域）
        _titleBar.MouseDoubleClick += e =>
        {
            if (e.Button == MouseButton.Left && CanMaximize)
            {
                if (e.GetPosition(_titleBar) is Point p &&
                    (_leftArea.Bounds.Contains(p) || _rightArea.Bounds.Contains(p)))
                {
                    e.Handled = true;
                    return;
                }
                if (WindowState == WindowState.Maximized) Restore();
                else Maximize();
                e.Handled = true;
            }
        };

        // 内容区域
        _contentArea = new Border { Padding = new Thickness(0) };

        _chromeBorder = new Border
        {
            BorderThickness = 0,
            Child = new DockPanel().Children(
                _titleBar.DockTop(),
                _contentArea
            )
        };
        _chromeBorder.SetBinding(Border.BorderBrushProperty, this, BorderBrushProperty);

        base.Content = _chromeBorder;

        ClientSizeChanged += _ =>
        {
            OnWindowStateVisualUpdate();
            UpdateChromeButtonVisibility();
        };

        Activated += UpdateChromeAppearance;
        Deactivated += UpdateChromeAppearance;
        Loaded += OnLoaded;
    }

    private void OnLoaded()
    {
        if (BorderBrush.A > 0
            && !ChromeCapabilities.HasFlag(WindowChromeCapabilities.NativeBorderColor)
            && !ChromeCapabilities.HasFlag(WindowChromeCapabilities.NativeWindowBorder))
        {
            _chromeBorder.BorderThickness = 1;
        }
        UpdateChromeButtonVisibility();
    }

    // ── 公开 API ─────────────────────────────────────────────────

    /// <summary>标题栏左侧区域（应用图标、菜单栏等）。</summary>
    public StackPanel TitleBarLeft => _leftArea;

    /// <summary>标题栏右侧区域（主题切换等额外操作）。</summary>
    public StackPanel TitleBarRight => _rightArea;

    public new UIElement? Content
    {
        get => _contentArea.Child;
        set => _contentArea.Child = value;
    }

    public new Thickness Padding
    {
        get => _contentArea.Padding;
        set => _contentArea.Padding = value;
    }

    // ── 内部实现 ─────────────────────────────────────────────────

    private void UpdateChromeAppearance()
    {
        var p = Theme.Palette;
        Background = p.WindowBackground;
        Foreground = p.WindowText;
        _contentArea.Background = p.WindowBackground;
        BorderBrush = IsActive ? p.Accent : p.ControlBorder;
        _titleText.Foreground = IsActive ? p.WindowText : p.DisabledText;
    }

    protected override void OnThemeChanged(Theme oldTheme, Theme newTheme)
    {
        base.OnThemeChanged(oldTheme, newTheme);
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
        bool maximized = WindowState == WindowState.Maximized;
        if (_maximizeBtn.Content is GlyphElement glyph)
            glyph.Kind = maximized ? GlyphKind.WindowRestore : GlyphKind.WindowMaximize;
    }

    // ── Chrome 按钮样式 ───────────────────────────────────────────

    private static readonly Style ChromeButtonStyle = new(typeof(Button))
    {
        Transitions = [Transition.Create(Control.BackgroundProperty), Transition.Create(Control.ForegroundProperty)],
        Setters =
        [
            Setter.Create(Control.BackgroundProperty, t => t.Palette.ButtonFace.WithAlpha(0)),
            Setter.Create(Control.ForegroundProperty, t => t.Palette.WindowText),
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

    private static readonly Style CloseButtonStyle = new(typeof(Button))
    {
        Transitions = [Transition.Create(Control.BackgroundProperty)],
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
                    Setter.Create(Control.ForegroundProperty, Color.White),
                ],
            },
            new StateTrigger
            {
                Match = VisualStateFlags.Pressed,
                Setters =
                [
                    Setter.Create(Control.BackgroundProperty, Color.FromRgb(200, 12, 28)),
                    Setter.Create(Control.ForegroundProperty, Color.White),
                ],
            },
        ],
    };

    private static Button CreateChromeButton(GlyphKind kind, bool isClose = false)
    {
        var glyph = new GlyphElement().Kind(kind).GlyphSize(ChromeButtonSize);
        return CreateChromeButton(glyph, isClose);
    }

    private static Button CreateChromeButton(Element content, bool isClose = false) =>
        new Button
        {
            Content = content,
            MinWidth = ButtonWidth,
            MinHeight = DefaultTitleBarHeight,
            StyleName = isClose ? "close" : "chrome",
        };

    /// <summary>
    /// Enables alpha-correct text rendering in DWM title bar regions.
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
