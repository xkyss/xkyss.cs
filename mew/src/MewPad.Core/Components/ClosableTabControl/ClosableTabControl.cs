namespace MewPad.Core.Components.ClosableTabControl;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;

/// <summary>
/// A <see cref="TabControl"/> wrapper that adds a close button to each tab header.
/// The close button is invisible by default and becomes visible on mouse hover
/// (same pattern as VS Code / Resty LabView).
///
/// Usage:
/// <code>
/// var tabs = new ClosableTabControl();
/// tabs.AddClosableTab("🎨 Card", content, onClose: () => { });
/// </code>
/// </summary>
public sealed class ClosableTabControl
{
    // Style for close button: invisible by default, visible on header hover via Trigger.
    // This replaces manual MouseEnter/MouseLeave binding which caused event leaks.
    private static readonly Style CloseButtonStyle = new(typeof(Button))
    {
        Transitions = [Transition.Create(Control.ForegroundProperty)],
        Setters =
        [
            Setter.Create(Control.BackgroundProperty, Color.FromRgb(0, 0, 0).WithAlpha(0)),
            Setter.Create(Control.ForegroundProperty, Color.FromRgb(0, 0, 0).WithAlpha(0)),
            Setter.Create(Control.BorderThicknessProperty, 0.0),
            Setter.Create(Control.PaddingProperty, new Thickness(0)),
            Setter.Create(Control.MinWidthProperty, 16.0),
            Setter.Create(Control.MinHeightProperty, 16.0),
        ],
        Triggers =
        [
            new StateTrigger
            {
                Match = VisualStateFlags.Hot,
                Setters =
                [
                    Setter.Create(Control.ForegroundProperty, t => t.Palette.WindowText),
                ],
            },
        ],
    };

    // All close buttons in insertion order, kept in sync with TabControl.Tabs.
    private readonly List<Button> _closeBtns = [];

    /// <summary>The underlying MewUI TabControl. Attach this to the visual tree.</summary>
    public TabControl Inner { get; } = new TabControl
    {
        VerticalScroll = ScrollMode.Disabled,
        HorizontalScroll = ScrollMode.Disabled,
    };

    /// <summary>Number of tabs currently open.</summary>
    public int Count => Inner.Tabs.Count;

    public ClosableTabControl()
    {
        Inner.StyleSheet = new StyleSheet();
        Inner.StyleSheet.Define("closebtn", CloseButtonStyle);
    }

    /// <summary>
    /// Adds a new tab with an optional close button.
    /// </summary>
    /// <param name="title">Text shown in the header.</param>
    /// <param name="icon">Optional icon string placed before the title.</param>
    /// <param name="content">The tab body element.</param>
    /// <param name="closable">Whether to show a close button on the tab header.</param>
    /// <param name="onClose">Called after the tab is removed. Only used when <paramref name="closable"/> is true.</param>
    /// <returns>The newly created <see cref="TabItem"/>.</returns>
    public TabItem AddClosableTab(string title, string? icon, FrameworkElement content, bool closable = true, Action? onClose = null)
    {
        TabItem? item = null;

        // ── Header ────────────────────────────────────────────────
        var titleRow = new StackPanel { Orientation = Orientation.Horizontal };
        titleRow.Spacing = 4;

        if (icon != null)
            titleRow.Children(new Label { Text = icon, FontSize = 12 });

        titleRow.Children(new Label { Text = title });

        if (closable)
        {
            var closeBtn = new Button
            {
                Content = new GlyphElement { Kind = GlyphKind.Cross, GlyphSize = 3.5, IsHitTestVisible = false },
                StyleName = "closebtn",
            };

            var capturedCloseBtn = closeBtn;
            closeBtn.Click += () =>
            {
                if (item == null) return;
                var idx = IndexOf(item);
                if (idx < 0) return;
                _closeBtns.RemoveAt(idx);
                Inner.RemoveTabAt(idx);
                onClose?.Invoke();
            };

            titleRow.Children(closeBtn);
            _closeBtns.Add(closeBtn);
        }
        else
        {
            // Placeholder so index stays in sync with Inner.Tabs.
            _closeBtns.Add(null!);
        }

        item = new TabItem { Header = titleRow, Content = content };
        Inner.AddTab(item);

        return item;
    }

    /// <summary>
    /// Removes the tab at the given index without invoking the onClose callback.
    /// </summary>
    public void RemoveAt(int index)
    {
        if ((uint)index >= (uint)Inner.Tabs.Count) return;
        _closeBtns.RemoveAt(index);
        Inner.RemoveTabAt(index);
    }

    /// <summary>Selects the tab at the given index.</summary>
    public void Select(int index) => Inner.SelectedIndex = index;

    /// <summary>Selects the last tab.</summary>
    public void SelectLast()
    {
        if (Inner.Tabs.Count > 0)
            Inner.SelectedIndex = Inner.Tabs.Count - 1;
    }

    // ── Private helpers ──────────────────────────────────────────────

    private int IndexOf(TabItem item)
    {
        for (var i = 0; i < Inner.Tabs.Count; i++)
            if (ReferenceEquals(Inner.Tabs[i], item)) return i;
        return -1;
    }
}
