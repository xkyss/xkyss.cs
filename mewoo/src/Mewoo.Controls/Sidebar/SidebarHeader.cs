using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;

namespace Mewoo.Controls.Sidebar;

public sealed class SidebarHeader
{
    private const double HeaderHeight = 38;
    private const double ActionSize = 28;
    private static readonly Color Transparent = Color.FromRgb(0, 0, 0).WithAlpha(0);

    private readonly string _title;
    private readonly SidebarControlOptions? _options;
    private readonly List<SidebarHeaderAction> _actions = [];
    private SidebarMoreMenuBuilder? _moreMenu;

    private SidebarHeader(string title, SidebarControlOptions? options)
    {
        _title = title;
        _options = options;
    }

    public static SidebarHeader Create(string title, SidebarControlOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Sidebar title is required.", nameof(title));
        }

        return new SidebarHeader(title, options);
    }

    public SidebarHeader Action(string icon, string label, Action action) =>
        Action(icon, label, _ =>
        {
            action();
            return ValueTask.CompletedTask;
        });

    public SidebarHeader Action(string icon, string label, Func<Task> action) =>
        Action(icon, label, async _ => await action());

    public SidebarHeader Action(string icon, string label, Func<CancellationToken, ValueTask> action) =>
        Action(new TextBlock().Text(icon).Center(), label, action);

    public SidebarHeader Action(PathGeometry icon, string label, Action action) =>
        Action(icon, label, _ =>
        {
            action();
            return ValueTask.CompletedTask;
        });

    public SidebarHeader Action(PathGeometry icon, string label, Func<Task> action) =>
        Action(icon, label, async _ => await action());

    public SidebarHeader Action(PathGeometry icon, string label, Func<CancellationToken, ValueTask> action) =>
        Action(PathIcon(icon, 14), label, action);

    public SidebarHeader Action(Element icon, string label, Action action) =>
        Action(icon, label, _ =>
        {
            action();
            return ValueTask.CompletedTask;
        });

    public SidebarHeader Action(Element icon, string label, Func<Task> action) =>
        Action(icon, label, async _ => await action());

    public SidebarHeader Action(Element icon, string label, Func<CancellationToken, ValueTask> action)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Sidebar header action label is required.", nameof(label));
        }

        _actions.Add(new SidebarHeaderAction(icon, label, action));
        return this;
    }

    public SidebarHeader More(Action<SidebarMoreMenuBuilder> configure)
    {
        var builder = new SidebarMoreMenuBuilder(_options);
        configure(builder);
        _moreMenu = builder;
        return this;
    }

    public Element Build()
    {
        var title = new TextBlock()
            .Text(_title)
            .SemiBold()
            .TextTrimming(TextTrimming.CharacterEllipsis)
            .CenterVertical()
            .Margin(12, 0, 8, 0);

        var actionGroup = new StackPanel { Orientation = Orientation.Horizontal }
            .Spacing(2)
            .Center();
        foreach (var action in _actions)
        {
            actionGroup.Children(CreateActionButton(action));
        }

        var right = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
        }
            .CenterVertical()
            .Margin(8, 0, 8, 0);
        if (_moreMenu is not null)
        {
            right.Add(CreateMoreMenu(_moreMenu.Build()));
        }

        var grid = new Grid()
            .Columns("*,auto,*");
        grid.Height = HeaderHeight;
        Grid.SetColumn(title, 0);
        Grid.SetColumn(actionGroup, 1);
        Grid.SetColumn(right, 2);
        grid.Children(title, actionGroup, right);
        return grid;
    }

    private Button CreateActionButton(SidebarHeaderAction action)
    {
        var button = new Button
        {
            Content = action.Icon,
            Width = ActionSize,
            Height = ActionSize,
            Padding = new Thickness(0),
            CornerRadius = 4,
            Background = Transparent,
            BorderBrush = Transparent,
            BorderThickness = 0,
        }.ToolTip(action.Label);
        button.OnClick(SidebarActionRunner.ForButton(button, action.Action, _options));
        return button;
    }

    private static MenuBar CreateMoreMenu(ContextMenu menu)
    {
        var menuItem = new MenuItem("...")
        {
            SubMenu = menu.Menu,
        };
        var menuBar = new MenuBar
        {
            Width = ActionSize,
            Height = ActionSize,
            Padding = new Thickness(0),
            CornerRadius = 4,
            Background = Transparent,
            BorderBrush = Transparent,
            BorderThickness = 0,
            DrawBottomSeparator = false,
        }.ToolTip("More");
        menuBar.Add(menuItem);
        return menuBar;
    }

    private static PathShape PathIcon(PathGeometry data, double size) => new PathShape()
        .Center()
        .Size(size)
        .Stretch(Stretch.Uniform)
        .WithTheme((theme, shape) => shape.Data(data).Fill(theme.Palette.WindowText));

    private sealed record SidebarHeaderAction(
        Element Icon,
        string Label,
        Func<CancellationToken, ValueTask> Action);
}
