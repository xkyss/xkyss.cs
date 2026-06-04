using Aprillz.MewUI.Controls;

namespace Mewoo.Controls.Sidebar;

public sealed class SidebarMoreMenuBuilder
{
    private readonly SidebarControlOptions? _options;
    private readonly ContextMenu _menu = new();

    internal SidebarMoreMenuBuilder(SidebarControlOptions? options)
    {
        _options = options;
    }

    public SidebarMoreMenuBuilder Item(string title, Action action, bool isEnabled = true) =>
        Item(title, _ =>
        {
            action();
            return ValueTask.CompletedTask;
        }, isEnabled);

    public SidebarMoreMenuBuilder Item(string title, Func<Task> action, bool isEnabled = true) =>
        Item(title, async _ => await action(), isEnabled);

    public SidebarMoreMenuBuilder Item(
        string title,
        Func<CancellationToken, ValueTask> action,
        bool isEnabled = true)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("More menu item title is required.", nameof(title));
        }

        _menu.AddItem(title, SidebarActionRunner.ForMenuItem(action, _options), isEnabled, shortcut: null);
        return this;
    }

    public SidebarMoreMenuBuilder Separator()
    {
        _menu.AddSeparator();
        return this;
    }

    internal ContextMenu Build() => _menu;
}

