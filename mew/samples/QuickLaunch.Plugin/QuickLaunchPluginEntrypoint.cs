namespace QuickLaunch.Plugin;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewPad.Core.Interfaces;
using MewPad.Core.Shell;

/// <summary>
/// QuickLaunch 示例插件入口。
///
/// 在动态加载器完成前，宿主可手动调用：
/// QuickLaunchPluginEntrypoint.Register(shell)
/// </summary>
public static class QuickLaunchPluginEntrypoint
{
    public static void Register(ShellContext shell)
    {
        shell.RegisterActivity(new QuickLaunchActivity(shell));
        shell.RegisterStatusBarItem(new StatusBarItem(
            "quicklaunch.ready",
            () => new Label { Text = "⚡ QuickLaunch", FontSize = 11, VerticalAlignment = VerticalAlignment.Center },
            StatusBarSlot.Right,
            Priority: 40));
    }
}

internal sealed class QuickLaunchActivity(ShellContext shell) : IActivityItem
{
    public string Id => "quicklaunch.home";
    public object Icon => "⚡";
    public string Title => "快捷启动";
    public ActivityBarSection Section => ActivityBarSection.Top;
    public int Order => 20;

    public FrameworkElement CreateContent()
    {
        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10),
        }.Children(
            new Label
            {
                Text = "快捷启动",
                FontSize = 16,
                FontWeight = FontWeight.SemiBold,
                Margin = new Thickness(0, 0, 0, 8),
            },
            new Label
            {
                Text = "示例插件：演示 Activity / Content / StatusBar 的最小接入。",
                FontSize = 11,
                Margin = new Thickness(0, 0, 0, 10),
            },
            new Button
            {
                Content = new Label { Text = "打开设置" },
                Margin = new Thickness(0, 0, 0, 6),
                MinWidth = 120,
            }.OnClick(() => shell.Settings.OpenSettings()),
            new Button
            {
                Content = new Label { Text = "切换主题" },
                Margin = new Thickness(0, 0, 0, 6),
                MinWidth = 120,
            }.OnClick(() => shell.Theme.Toggle()),
            new Button
            {
                Content = new Label { Text = "打开插件说明页" },
                Margin = new Thickness(0, 0, 0, 6),
                MinWidth = 120,
            }.OnClick(() => shell.OpenContent(new QuickLaunchGuideContent()))
        );
    }
}

internal sealed class QuickLaunchGuideContent : IContentItem
{
    public string Id => "quicklaunch.guide";
    public string Title => "QuickLaunch Guide";
    public object? Icon => "⚡";
    public bool CanClose => true;

    public FrameworkElement CreateContent()
    {
        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(20),
        }.Children(
            new Label
            {
                Text = "QuickLaunch 插件说明",
                FontSize = 18,
                FontWeight = FontWeight.SemiBold,
                Margin = new Thickness(0, 0, 0, 12),
            },
            new Label { Text = "- 通过 ActivityBar 提供入口", FontSize = 12 },
            new Label { Text = "- 通过 OpenContent 打开插件内容页", FontSize = 12 },
            new Label { Text = "- 通过 StatusBarItem 暴露插件状态", FontSize = 12 }
        );
    }
}
