namespace MewPad.Core.Components.ClosableTabControl;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

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
        Button? closeBtn = null;

        // ── Header ────────────────────────────────────────────────
        var titleRow = new StackPanel { Orientation = Orientation.Horizontal };
        titleRow.Spacing = 4;

        if (icon != null)
            titleRow.Children(new Label { Text = icon, FontSize = 12 });

        titleRow.Children(new Label { Text = title });

        if (closable)
        {
            closeBtn = new Button
            {
                Content = new GlyphElement { Kind = GlyphKind.Cross, GlyphSize = 3.5, IsHitTestVisible = false },
                MinWidth = 16,
                MinHeight = 16,
                Padding = new Thickness(0),
                BorderThickness = 0,
            };
            closeBtn.WithTheme((t, b) => b.Background = Color.FromRgb(0, 0, 0).WithAlpha(0));
            // Initially transparent — shown on header hover via BindTabHoverEvents().
            closeBtn.WithTheme((t, b) => b.Foreground = Color.FromRgb(0, 0, 0).WithAlpha(0));

            var capturedBtn = closeBtn;
            closeBtn.Click += () =>
            {
                if (item == null) return;
                var idx = IndexOf(item);
                if (idx < 0) return;
                _closeBtns.RemoveAt(idx);
                Inner.RemoveTabAt(idx);
                BindTabHoverEvents();
                onClose?.Invoke();
            };

            titleRow.Children(capturedBtn);
            _closeBtns.Add(closeBtn);
        }
        else
        {
            // Placeholder so index stays in sync with Inner.Tabs.
            _closeBtns.Add(null!);
        }

        item = new TabItem { Header = titleRow, Content = content };
        Inner.AddTab(item);

        // Rebind hover events whenever the header strip is rebuilt.
        BindTabHoverEvents();

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
        BindTabHoverEvents();
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

    /// <summary>
    /// Re-walks the visual tree after every structural change to bind mouse-enter/leave
    /// on each TabHeaderButton so the matching close button fades in/out.
    ///
    /// MewUI rebuilds all TabHeaderButton instances on AddTab/RemoveTabAt, so we must
    /// rebind after every mutation (same approach as Resty LabView).
    /// </summary>
    private void BindTabHoverEvents()
    {
        var index = 0;
        VisualTree.Visit(Inner, el =>
        {
            if (el.GetType().Name == "TabHeaderButton" && el is UIElement thb)
            {
                if (index >= _closeBtns.Count) return;
                var btn = _closeBtns[index++];
                if (btn == null) return; // non-closable tab, skip hover binding

                // Show/hide close button by making it visible/invisible via foreground.
                thb.MouseEnter += () => btn.WithTheme((t, b) =>
                    b.Foreground = t.Palette.WindowText);
                thb.MouseLeave += () => btn.WithTheme((t, b) =>
                    b.Foreground = Color.FromRgb(0, 0, 0).WithAlpha(0));
            }
        });
    }
}
