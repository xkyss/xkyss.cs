namespace ComponentsDemo.Plugin;

using Aprillz.MewUI.Controls;
using ComponentsDemo.Plugin.UI;
using MewPad.Core.Interfaces;
using MewPad.Core.Plugins;
using MewPad.Core.Shell;

/// <summary>
/// ComponentsDemo 插件入口。
/// 用于在 MewPad 宿主中演示 MewPad.Core 组件库的各种组件。
/// </summary>
public static class ComponentsDemoPluginEntrypoint
{
    public static void Register(ShellContext shell)
    {
        new ComponentsDemoPlugin().Register(shell);
    }
}

public sealed class ComponentsDemoPlugin : IPlugin
{
    private static int s_registered;

    public string Id => "components.demo";
    public string MinHostVersion => "0.1.0";
    public string[] Dependencies => [];

    public void Register(ShellContext shell)
    {
        if (Interlocked.Exchange(ref s_registered, 1) == 1)
        {
            return;
        }

        shell.RegisterActivity(new ComponentsDemoActivity(shell));
    }
}

internal sealed class ComponentsDemoActivity(ShellContext shell) : IActivityItem
{
    public string Id => "components.demo.home";
    public object Icon => "🎨";
    public string Title => "组件演示";
    public ActivityBarSection Section => ActivityBarSection.Bottom;
    public int Order => 10;

    public FrameworkElement CreateContent()
    {
        return new ComponentsDemoSidebar(shell).Build();
    }
}
