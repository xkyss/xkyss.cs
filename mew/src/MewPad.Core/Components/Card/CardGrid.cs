namespace MewPad.Core.Components.Card;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Input;
using Aprillz.MewUI.Rendering;

/// <summary>
/// Represents a grid-item card container used inside card body layouts.
/// </summary>
public sealed class CardGrid : ContentControl
{
    private bool _isPressed;

    public static readonly MewProperty<bool> HoverableProperty =
        MewProperty<bool>.Register<CardGrid>(nameof(Hoverable), true, MewPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets whether hover/press feedback is enabled.
    /// </summary>
    public bool Hoverable
    {
        get => GetValue(HoverableProperty);
        set => SetValue(HoverableProperty, value);
    }

    public override bool Focusable => true;

    public event Action? Clicked;

    protected override VisualState ComputeVisualState()
    {
        var state = base.ComputeVisualState();
        if (!Hoverable)
        {
            var flags = state.Flags & ~VisualStateFlags.Hot & ~VisualStateFlags.Pressed;
            return state with { Flags = flags };
        }

        return state;
    }

    protected override void OnRender(IGraphicsContext context)
    {
        var state = CurrentVisualState;
        var theme = Theme;

        var bg = theme.Palette.ContainerBackground;
        if (Hoverable && state.IsHot)
        {
            bg = bg.Lerp(theme.Palette.Accent, 0.03);
        }

        if (Hoverable && state.IsPressed)
        {
            bg = bg.Lerp(theme.Palette.Accent, 0.06);
        }

        var border = theme.Palette.ControlBorder;
        if (Hoverable && state.IsHot)
        {
            border = Color.Composite(border, theme.Palette.AccentBorderHotOverlay);
        }

        if (Hoverable && (state.IsPressed || state.IsFocused))
        {
            border = theme.Palette.Accent;
        }

        DrawBackgroundAndBorder(context, GetSnappedBorderBounds(Bounds), bg, border, BorderThickness, CornerRadius);

        base.RenderSubtree(context);
    }

    protected override void RenderSubtree(IGraphicsContext context)
    {
        // OnRender already renders the content after custom background/border.
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Handled || e.Button != MouseButton.Left || !IsEffectivelyEnabled)
        {
            return;
        }

        _isPressed = true;
        SetPressed(true);
        Focus();

        if (FindVisualRoot() is Window window)
        {
            window.CaptureMouse(this);
        }

        e.Handled = true;
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (!_isPressed || e.Button != MouseButton.Left)
        {
            return;
        }

        _isPressed = false;
        SetPressed(false);

        if (FindVisualRoot() is Window window)
        {
            window.ReleaseMouseCapture();
        }

        if (!e.Handled && IsEffectivelyEnabled && Bounds.Contains(e.GetPosition(this)))
        {
            Clicked?.Invoke();
            e.Handled = true;
        }
    }

    protected override void OnMouseLeave()
    {
        base.OnMouseLeave();
        _isPressed = false;
        SetPressed(false);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (!IsEffectivelyEnabled || e.Handled)
        {
            return;
        }

        if (e.Key is Key.Space or Key.Enter)
        {
            _isPressed = true;
            SetPressed(true);
            e.Handled = true;
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (!IsEffectivelyEnabled || !_isPressed)
        {
            return;
        }

        if (e.Key is Key.Space or Key.Enter)
        {
            _isPressed = false;
            SetPressed(false);
            Clicked?.Invoke();
            e.Handled = true;
        }
    }
}
