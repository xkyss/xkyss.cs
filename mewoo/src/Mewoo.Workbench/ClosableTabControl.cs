using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;

namespace Mewoo.Workbench;

internal sealed class ClosableTabControl
{
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

    private readonly List<Button?> _closeButtons = [];

    public TabControl Inner { get; } = new()
    {
        VerticalScroll = ScrollMode.Disabled,
        HorizontalScroll = ScrollMode.Disabled,
    };

    public int Count => Inner.Tabs.Count;

    public ClosableTabControl()
    {
        Inner.StyleSheet = new StyleSheet();
        Inner.StyleSheet.Define("closebtn", CloseButtonStyle);
    }

    public TabItem AddTab(string title, FrameworkElement content, bool closable, Action? onClose)
    {
        TabItem? item = null;

        var titleRow = new StackPanel { Orientation = Orientation.Horizontal };
        titleRow.Spacing = 4;
        titleRow.Children(new Label { Text = title });

        if (closable)
        {
            var closeButton = new Button
            {
                Content = new GlyphElement { Kind = GlyphKind.Cross, GlyphSize = 3.5, IsHitTestVisible = false },
                StyleName = "closebtn",
            };

            closeButton.Click += () =>
            {
                if (item is null)
                {
                    return;
                }

                var index = IndexOf(item);
                if (index < 0)
                {
                    return;
                }

                RemoveAt(index);
                onClose?.Invoke();
            };

            titleRow.Children(closeButton);
            _closeButtons.Add(closeButton);
        }
        else
        {
            _closeButtons.Add(null);
        }

        item = new TabItem { Header = titleRow, Content = content };
        Inner.AddTab(item);
        return item;
    }

    public void Clear()
    {
        while (Inner.Tabs.Count > 0)
        {
            RemoveAt(0);
        }
    }

    public void RemoveAt(int index)
    {
        if ((uint)index >= (uint)Inner.Tabs.Count)
        {
            return;
        }

        _closeButtons.RemoveAt(index);
        Inner.RemoveTabAt(index);
    }

    public void Select(int index)
    {
        if ((uint)index < (uint)Inner.Tabs.Count)
        {
            Inner.SelectedIndex = index;
        }
    }

    public void SelectLast()
    {
        if (Inner.Tabs.Count > 0)
        {
            Inner.SelectedIndex = Inner.Tabs.Count - 1;
        }
    }

    private int IndexOf(TabItem item)
    {
        for (var i = 0; i < Inner.Tabs.Count; i++)
        {
            if (ReferenceEquals(Inner.Tabs[i], item))
            {
                return i;
            }
        }

        return -1;
    }
}

