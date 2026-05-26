using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;

namespace Mewoo.Workbench;

public class MewooNativeWindow : Window
{
    private const double TitleBarHeight = 34;
    private const double ButtonWidth = 42;
    private const double ChromeGlyphSize = 4;

    private readonly Border _contentArea;
    private readonly AlphaTextPanel _titleBar;
    private readonly TextBlock _titleText;
    private readonly StackPanel _leftArea;
    private readonly StackPanel _rightArea;
    private readonly StackPanel _controlButtons;
    private readonly Button _maximizeButton;

    private static readonly Style ChromeButtonStyle = new(typeof(Button))
    {
        Transitions = [Transition.Create(Control.BackgroundProperty)],
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
        ],
    };

    public MewooNativeWindow()
    {
        ExtendClientAreaTitleBarHeight = TitleBarHeight;
        base.Padding = new Thickness(0);

        StyleSheet = new StyleSheet();
        StyleSheet.Define("mewoo.chrome", ChromeButtonStyle);
        StyleSheet.Define("mewoo.close", CloseButtonStyle);

        _titleText = new TextBlock
        {
            IsHitTestVisible = false,
            FontWeight = FontWeight.SemiBold,
            FontSize = 13,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _titleText.SetBinding(TextBlock.TextProperty, this, TitleProperty);

        var minimizeButton = CreateChromeButton(GlyphKind.WindowMinimize);
        minimizeButton.Click += Minimize;
        minimizeButton.SetBinding(UIElement.IsVisibleProperty, this, CanMinimizeProperty);

        var maximizeGlyph = new GlyphElement().Kind(GlyphKind.WindowMaximize).GlyphSize(ChromeGlyphSize);
        _maximizeButton = CreateChromeButton(maximizeGlyph);
        _maximizeButton.Click += ToggleMaximize;
        _maximizeButton.SetBinding(UIElement.IsVisibleProperty, this, CanMaximizeProperty);

        var closeButton = CreateChromeButton(GlyphKind.Cross, isClose: true);
        closeButton.Click += Close;
        closeButton.SetBinding(UIElement.IsVisibleProperty, this, CanCloseProperty);

        _controlButtons = new StackPanel { Orientation = Orientation.Horizontal };
        _controlButtons.Children(minimizeButton, _maximizeButton, closeButton);

        _leftArea = new StackPanel { Orientation = Orientation.Horizontal };
        _rightArea = new StackPanel { Orientation = Orientation.Horizontal };

        _titleBar = new AlphaTextPanel
        {
            MinHeight = TitleBarHeight,
            Content = new DockPanel().Children(
                new Border().DockRight().Child(_controlButtons),
                new Border().DockRight().Child(_rightArea),
                new Border().DockLeft().Child(_leftArea),
                _titleText)
        };
        _titleBar.SetBinding(BackgroundProperty, this, BackgroundProperty);
        _titleBar.MouseDoubleClick += e =>
        {
            if (e.Button != MouseButton.Left || !CanMaximize)
            {
                return;
            }

            if (e.GetPosition(_titleBar) is Point p && (_leftArea.Bounds.Contains(p) || _rightArea.Bounds.Contains(p)))
            {
                e.Handled = true;
                return;
            }

            ToggleMaximize();
            e.Handled = true;
        };

        _contentArea = new Border();

        base.Content = new Border
        {
            BorderThickness = 0,
            Child = new DockPanel().Children(
                _titleBar.DockTop(),
                _contentArea)
        };

        ClientSizeChanged += _ => UpdateWindowStateVisuals();
        Loaded += UpdateChromeVisibility;
    }

    public StackPanel TitleBarLeft => _leftArea;

    public StackPanel TitleBarRight => _rightArea;

    public new UIElement? Content
    {
        get => _contentArea.Child;
        set => _contentArea.Child = value;
    }

    private void ToggleMaximize()
    {
        if (WindowState == WindowState.Maximized)
        {
            Restore();
        }
        else
        {
            Maximize();
        }
    }

    private void UpdateChromeVisibility()
    {
        _titleBar.IsVisible = ChromeCapabilities.HasFlag(WindowChromeCapabilities.ExtendClientArea);
        _controlButtons.IsVisible = !HasNativeChromeButtons;
        _titleBar.Padding = NativeChromeButtonInset;
    }

    private void UpdateWindowStateVisuals()
    {
        if (_maximizeButton.Content is GlyphElement glyph)
        {
            glyph.Kind = WindowState == WindowState.Maximized
                ? GlyphKind.WindowRestore
                : GlyphKind.WindowMaximize;
        }

        UpdateChromeVisibility();
    }

    private static Button CreateChromeButton(GlyphKind kind, bool isClose = false)
    {
        return CreateChromeButton(new GlyphElement().Kind(kind).GlyphSize(ChromeGlyphSize), isClose);
    }

    private static Button CreateChromeButton(Element content, bool isClose = false)
    {
        return new Button
        {
            Content = content,
            MinWidth = ButtonWidth,
            MinHeight = TitleBarHeight,
            StyleName = isClose ? "mewoo.close" : "mewoo.chrome",
        };
    }

    private sealed class AlphaTextPanel : ContentControl
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

