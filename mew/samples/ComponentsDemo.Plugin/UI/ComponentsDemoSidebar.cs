namespace ComponentsDemo.Plugin.UI;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using ComponentsDemo.Plugin.UI.Demos;
using MewPad.Core.Interfaces;
using MewPad.Core.Shell;

/// <summary>
/// Sidebar navigation for the Components Demo plugin.
/// Lists available component demos; clicking opens a content tab.
/// </summary>
internal sealed class ComponentsDemoSidebar(ShellContext shell)
{
    private static readonly (string Id, string Label, string Icon, Func<IContentItem> Factory)[] s_demos =
    [
        ("card",        "Card 卡片",           "🃏", () => new CardDemoContent()),
        ("quicklaunch", "QuickLaunch 快速启动", "🚀", () => new QuickLaunchDemoContent()),
    ];

    public FrameworkElement Build()
    {
        var root = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0,
        };

        var header = new Label
        {
            Text = "组件",
            FontSize = 11,
            Margin = new Thickness(12, 10, 12, 6),
            Foreground = default, // inherits from theme via WithTheme below
        };

        root.Add(header);
        root.WithTheme((t, _) => header.Foreground = t.Palette.DisabledText);

        foreach (var (id, label, icon, factory) in s_demos)
        {
            var btn = BuildDemoButton(id, label, icon, factory);
            root.Add(btn);
        }

        return root;
    }

    private Button BuildDemoButton(string id, string label, string icon, Func<IContentItem> factory)
    {
        var contentId = $"components.demo.{id}";
        var btn = new Button
        {
            Content = new Label { Text = $"{icon}  {label}", FontSize = 13 },
            Margin = new Thickness(6, 2, 6, 2),
            MinHeight = 32,
        };
        btn.Click += () =>
        {
            var item = factory();
            shell.OpenContent(item);
        };
        return btn;
    }
}
