namespace MewPad.Core.Components.Card;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Input;
using Aprillz.MewUI.Rendering;

public enum CardVariant
{
    Outlined,
    Borderless,
}

public enum CardSize
{
    Small,
    Medium,
}

public enum CardType
{
    Default,
    Inner,
}

/// <summary>
/// Represents a reusable card container with header, cover, tabs, body, and actions areas.
/// </summary>
public sealed class Card : Control, IVisualTreeHost
{
    private readonly StackPanel _layoutRoot;
    private readonly List<CardTabItem> _tabs = new();
    private readonly List<Element> _actions = new();

    private UIElement? _title;
    private UIElement? _extra;
    private UIElement? _cover;
    private UIElement? _body;
    private string? _activeTabKey;
    private string? _defaultActiveTabKey;
    private bool _loading;
    private bool _hoverable;
    private bool _bordered = true;
    private CardVariant _variant = CardVariant.Outlined;
    private CardSize _size = CardSize.Medium;
    private CardType _type = CardType.Default;

    private bool _isPressed;

    public Card()
    {
        _layoutRoot = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0,
        };
        AttachChild(_layoutRoot);

        Rebuild();
    }

    public override bool Focusable => true;

    /// <summary>
    /// Gets or sets the title element in the header area.
    /// </summary>
    public UIElement? Title
    {
        get => _title;
        set
        {
            if (!ReferenceEquals(_title, value))
            {
                _title = value;
                Rebuild();
            }
        }
    }

    /// <summary>
    /// Gets or sets the extra element in the header's right side.
    /// </summary>
    public UIElement? Extra
    {
        get => _extra;
        set
        {
            if (!ReferenceEquals(_extra, value))
            {
                _extra = value;
                Rebuild();
            }
        }
    }

    /// <summary>
    /// Gets or sets the cover element.
    /// </summary>
    public UIElement? Cover
    {
        get => _cover;
        set
        {
            if (!ReferenceEquals(_cover, value))
            {
                _cover = value;
                Rebuild();
            }
        }
    }

    /// <summary>
    /// Gets or sets the body element when tabs are not used.
    /// </summary>
    public UIElement? Body
    {
        get => _body;
        set
        {
            if (!ReferenceEquals(_body, value))
            {
                _body = value;
                Rebuild();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether loading placeholder should be shown in the body area.
    /// </summary>
    public bool Loading
    {
        get => _loading;
        set
        {
            if (_loading != value)
            {
                _loading = value;
                Rebuild();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether hover/press visual behavior is enabled.
    /// </summary>
    public bool Hoverable
    {
        get => _hoverable;
        set
        {
            if (_hoverable != value)
            {
                _hoverable = value;
                InvalidateVisual();
            }
        }
    }

    /// <summary>
    /// Gets or sets the card visual variant.
    /// </summary>
    public CardVariant Variant
    {
        get => _variant;
        set
        {
            if (_variant != value)
            {
                _variant = value;
                InvalidateVisual();
            }
        }
    }

    /// <summary>
    /// Gets or sets the card size.
    /// </summary>
    public CardSize Size
    {
        get => _size;
        set
        {
            if (_size != value)
            {
                _size = value;
                Rebuild();
            }
        }
    }

    /// <summary>
    /// Gets or sets the card type.
    /// </summary>
    public CardType Type
    {
        get => _type;
        set
        {
            if (_type != value)
            {
                _type = value;
                Rebuild();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether border is drawn.
    /// </summary>
    public bool Bordered
    {
        get => _bordered;
        set
        {
            if (_bordered != value)
            {
                _bordered = value;
                InvalidateVisual();
            }
        }
    }

    /// <summary>
    /// Gets all tab definitions.
    /// </summary>
    public IReadOnlyList<CardTabItem> Tabs => _tabs;

    /// <summary>
    /// Gets all action elements.
    /// </summary>
    public IReadOnlyList<Element> Actions => _actions;

    /// <summary>
    /// Gets or sets the currently active tab key.
    /// </summary>
    public string? ActiveTabKey
    {
        get => _activeTabKey;
        set
        {
            if (!string.Equals(_activeTabKey, value, StringComparison.Ordinal))
            {
                _activeTabKey = value;
                Rebuild();
            }
        }
    }

    /// <summary>
    /// Gets or sets the default active tab key used when <see cref="ActiveTabKey"/> is null.
    /// </summary>
    public string? DefaultActiveTabKey
    {
        get => _defaultActiveTabKey;
        set
        {
            if (!string.Equals(_defaultActiveTabKey, value, StringComparison.Ordinal))
            {
                _defaultActiveTabKey = value;
                Rebuild();
            }
        }
    }

    public event Action<string>? TabChanged;

    public event Action<int>? ActionInvoked;

    public event Action? Clicked;

    public void SetTabs(IEnumerable<CardTabItem>? tabs)
    {
        _tabs.Clear();
        if (tabs != null)
        {
            foreach (var tab in tabs)
            {
                if (tab != null)
                {
                    _tabs.Add(tab);
                }
            }
        }

        Rebuild();
    }

    public void SetActions(IEnumerable<Element>? actions)
    {
        _actions.Clear();
        if (actions != null)
        {
            foreach (var action in actions)
            {
                if (action != null)
                {
                    _actions.Add(action);
                }
            }
        }

        Rebuild();
    }

    public void SetTabs(params CardTabItem[] tabs) => SetTabs((IEnumerable<CardTabItem>)tabs);

    public void SetActions(params Element[] actions) => SetActions((IEnumerable<Element>)actions);

    protected override Size MeasureContent(Size availableSize)
    {
        var borderInset = GetBorderVisualInset();
        var border = borderInset > 0 ? new Thickness(borderInset) : Thickness.Zero;
        var slot = availableSize.Deflate(Padding).Deflate(border);
        _layoutRoot.Measure(slot);
        return _layoutRoot.DesiredSize.Inflate(Padding).Inflate(border);
    }

    protected override void ArrangeContent(Rect bounds)
    {
        var borderInset = GetBorderVisualInset();
        var border = borderInset > 0 ? new Thickness(borderInset) : Thickness.Zero;
        var contentBounds = bounds.Deflate(Padding).Deflate(border);
        _layoutRoot.Arrange(contentBounds);
    }

    protected override void OnRender(IGraphicsContext context)
    {
        var state = CurrentVisualState;
        var theme = Theme;
        var background = ResolveBackground(theme, state);

        var drawBorder = IsBorderVisible();
        var borderThickness = drawBorder ? BorderThickness : 0.0;
        var borderBrush = drawBorder ? ResolveBorderBrush(theme, state) : Color.Transparent;

        var bounds = GetSnappedBorderBounds(Bounds);
        DrawBackgroundAndBorder(context, bounds, background, borderBrush, borderThickness, CornerRadius);
    }

    protected override void RenderSubtree(IGraphicsContext context)
    {
        _layoutRoot.Render(context);
    }

    protected override UIElement? OnHitTest(Point point)
    {
        if (!IsVisible || !IsHitTestVisible)
        {
            return null;
        }

        var childHit = _layoutRoot.HitTest(point);
        if (childHit != null)
        {
            return childHit;
        }

        return Bounds.Contains(point) ? this : null;
    }

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

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Handled || e.Button != MouseButton.Left || !IsEffectivelyEnabled || !Hoverable)
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

        if (!e.Handled && Bounds.Contains(e.GetPosition(this)) && IsEffectivelyEnabled)
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

    bool IVisualTreeHost.VisitChildren(Func<Element, bool> visitor)
        => visitor(_layoutRoot);

    private void Rebuild()
    {
        _layoutRoot.Clear();

        var hasHeader = Title != null || Extra != null;
        if (hasHeader)
        {
            _layoutRoot.Add(BuildHeader());
        }

        if (Cover != null)
        {
            _layoutRoot.Add(BuildCover());
        }

        if (_tabs.Count > 0)
        {
            _layoutRoot.Add(BuildTabsBody());
        }
        else
        {
            _layoutRoot.Add(BuildBody());
        }

        if (_actions.Count > 0)
        {
            _layoutRoot.Add(BuildActions());
        }

        InvalidateMeasure();
        InvalidateVisual();
    }

    private Element BuildHeader()
    {
        var row = new DockPanel();

        if (Extra != null)
        {
            DockPanel.SetDock(Extra, Dock.Right);
            row.Add(Extra);
        }

        if (Title != null)
        {
            row.Add(Title);
        }

        return new Border
        {
            Padding = GetHeaderPadding(),
            BorderBrush = Theme.Palette.ControlBorder,
            BorderThickness = 0,
            Child = row,
        };
    }

    private Element BuildCover()
    {
        return new Border
        {
            Padding = Thickness.Zero,
            Child = Cover,
        };
    }

    private Element BuildBody()
    {
        return new Border
        {
            Padding = GetBodyPadding(),
            Child = BuildBodyContent(),
        };
    }

    private Element BuildTabsBody()
    {
        var tabs = new TabControl
        {
            VerticalScroll = ScrollMode.Disabled,
            HorizontalScroll = ScrollMode.Disabled,
            BorderThickness = 0,
            CornerRadius = 0,
            Padding = GetBodyPadding(),
            Background = Color.Transparent,
        };

        for (int i = 0; i < _tabs.Count; i++)
        {
            var tab = _tabs[i];
            tabs.AddTab(new TabItem
            {
                Header = tab.Header ?? new Label { Text = tab.Key },
                Content = tab.Content ?? new Label { Text = string.Empty },
                IsEnabled = tab.IsEnabled,
            });
        }

        var initialIndex = ResolveInitialTabIndex();
        if (initialIndex >= 0)
        {
            tabs.SelectedIndex = initialIndex;
        }

        tabs.SelectionChanged += _ =>
        {
            var selectedIndex = tabs.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < _tabs.Count)
            {
                var key = _tabs[selectedIndex].Key;
                if (!string.Equals(_activeTabKey, key, StringComparison.Ordinal))
                {
                    _activeTabKey = key;
                    TabChanged?.Invoke(key);
                }
            }
        };

        return tabs;
    }

    private Element BuildActions()
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
        };

        for (int i = 0; i < _actions.Count; i++)
        {
            var action = _actions[i];
            if (action is Button button)
            {
                var index = i;
                button.Click += () => ActionInvoked?.Invoke(index);
                row.Add(button);
            }
            else
            {
                row.Add(action);
            }
        }

        return new Border
        {
            Padding = GetActionsPadding(),
            Child = row,
        };
    }

    private UIElement BuildBodyContent()
    {
        if (Loading)
        {
            return BuildLoadingPlaceholder();
        }

        if (Body is UIElement body)
        {
            return body;
        }

        return new Label { Text = string.Empty };
    }

    private UIElement BuildLoadingPlaceholder()
    {
        var lineColor = Theme.Palette.ControlBorder.WithAlpha(96);

        var line1 = new Border { Height = 10, CornerRadius = 4, Background = lineColor, Margin = new Thickness(0, 0, 0, 6) };
        var line2 = new Border { Height = 10, CornerRadius = 4, Background = lineColor, Margin = new Thickness(0, 0, 24, 6) };
        var line3 = new Border { Height = 10, CornerRadius = 4, Background = lineColor, Margin = new Thickness(0, 0, 60, 0) };

        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0,
        };

        stack.Add(line1);
        stack.Add(line2);
        stack.Add(line3);

        return stack;
    }

    private int ResolveInitialTabIndex()
    {
        if (_tabs.Count == 0)
        {
            return -1;
        }

        var key = ActiveTabKey;
        if (string.IsNullOrEmpty(key))
        {
            key = DefaultActiveTabKey;
        }

        if (!string.IsNullOrEmpty(key))
        {
            for (int i = 0; i < _tabs.Count; i++)
            {
                if (string.Equals(_tabs[i].Key, key, StringComparison.Ordinal))
                {
                    return i;
                }
            }
        }

        return 0;
    }

    private Thickness GetHeaderPadding()
    {
        if (Type == CardType.Inner)
        {
            return Size == CardSize.Small ? new Thickness(8, 6, 8, 6) : new Thickness(12, 8, 12, 8);
        }

        return Size == CardSize.Small ? new Thickness(12, 8, 12, 8) : new Thickness(16, 12, 16, 12);
    }

    private Thickness GetBodyPadding()
    {
        if (Type == CardType.Inner)
        {
            return Size == CardSize.Small ? new Thickness(8) : new Thickness(12);
        }

        return Size == CardSize.Small ? new Thickness(12) : new Thickness(16);
    }

    private Thickness GetActionsPadding()
    {
        return Size == CardSize.Small
            ? new Thickness(12, 8, 12, 8)
            : new Thickness(16, 10, 16, 10);
    }

    private bool IsBorderVisible()
    {
        if (!Bordered)
        {
            return false;
        }

        return Variant != CardVariant.Borderless;
    }

    private Color ResolveBackground(Theme theme, in VisualState state)
    {
        var bg = theme.Palette.ContainerBackground;

        if (!state.IsEnabled)
        {
            return theme.Palette.DisabledControlBackground;
        }

        if (!Hoverable)
        {
            return bg;
        }

        if (state.IsPressed)
        {
            return bg.Lerp(theme.Palette.Accent, 0.06);
        }

        if (state.IsHot)
        {
            return bg.Lerp(theme.Palette.Accent, 0.035);
        }

        return bg;
    }

    private Color ResolveBorderBrush(Theme theme, in VisualState state)
    {
        var baseBorder = theme.Palette.ControlBorder;

        if (!state.IsEnabled)
        {
            return baseBorder;
        }

        if (!Hoverable)
        {
            return baseBorder;
        }

        if (state.IsFocused || state.IsPressed)
        {
            return theme.Palette.Accent;
        }

        if (state.IsHot)
        {
            return Color.Composite(baseBorder, theme.Palette.AccentBorderHotOverlay);
        }

        return baseBorder;
    }
}
