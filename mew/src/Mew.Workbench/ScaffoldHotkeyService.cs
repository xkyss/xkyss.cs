namespace Mew.Workbench;

/// <summary>
/// 临时占位热键服务(票据 07 宿主中央注册表落地后移除):满足 <see cref="ToolModuleContext"/> 契约用,
/// 全部空转。本阶段模块不消费 context.Hotkeys(每项热键仍走 ItemHotkeys 直连 GlobalHotkey),
/// 宿主/测试先用占位实现跑通组装路径。
/// </summary>
public sealed class ScaffoldHotkeyService : IHotkeyService
{
    public bool Register(IntPtr hwnd, string hotkey, Action callback) => true;

    public void Unregister(string hotkey)
    {
    }

    public bool IsRegistered(string hotkey) => false;
}
