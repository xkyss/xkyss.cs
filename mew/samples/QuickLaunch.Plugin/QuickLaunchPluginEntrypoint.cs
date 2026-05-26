namespace QuickLaunch.Plugin;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewPad.Core.Interfaces;
using MewPad.Core.Plugins;
using MewPad.Core.Shell;
using QuickLaunch.Plugin.Services;
using QuickLaunch.Plugin.UI;

/// <summary>
/// QuickLaunch 插件入口。
///
/// 在动态加载器完成前，宿主可手动调用：
/// QuickLaunchPluginEntrypoint.Register(shell)
/// </summary>
public static class QuickLaunchPluginEntrypoint
{
    public static void Register(ShellContext shell)
    {
        new QuickLaunchPlugin().Register(shell);
    }
}

public sealed class QuickLaunchPlugin : IPlugin
{
    private static int s_registered;

    public string Id => "quicklaunch.plugin";
    public string MinHostVersion => "0.1.0";
    public string[] Dependencies => [];

    public void Register(ShellContext shell)
    {
        // Loader may discover both IPlugin and static Register entrypoints.
        // Guard to ensure this plugin registers only once per process.
        if (Interlocked.Exchange(ref s_registered, 1) == 1)
        {
            return;
        }

        var launchService = new LaunchService();
        shell.RegisterActivity(new QuickLaunchActivity(shell, launchService));
        shell.RegisterStatusBarItem(new StatusBarItem(
            "quicklaunch.ready",
            () => new Label { Text = "⚡ QuickLaunch", FontSize = 11, VerticalAlignment = VerticalAlignment.Center },
            StatusBarSlot.Right,
            Priority: 40));
    }
}

internal sealed class QuickLaunchActivity(ShellContext shell, LaunchService launchService) : IActivityItem
{
    private readonly QuickLaunchPanel _panel = new QuickLaunchPanel(shell, launchService);

    public string Id => "quicklaunch.home";
    public object Icon => "⚡";
    public string Title => "快捷启动";
    public ActivityBarSection Section => ActivityBarSection.Top;
    public int Order => 20;

    public FrameworkElement CreateContent()
    {
            var rootPanel = new StackPanel().Vertical();

        // SideBar：两级分类导航
        var panelContent = _panel.CreateContent();
            rootPanel.Children(panelContent);

        // 监听分类选择事件，打开对应的 ContentArea
        _panel.CategorySelected += (categoryId) =>
        {
            var contentItem = new QuickLaunchContent(shell, launchService, categoryId);
            shell.OpenContent(contentItem);
        };

        return rootPanel;
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
