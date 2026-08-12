using Mew.Workbench;

namespace Mew.Launcher;

/// <summary>
/// 临时占位热键服务:宿主中央热键服务(票据 07)落地前满足 ToolModuleContext 契约用。
/// 本阶段模块不消费 context.Hotkeys(每项热键仍走 ItemHotkeys 直连 GlobalHotkey),占位实现全部空转。
/// 票据 06 宿主接管引导后移除。
/// </summary>
internal sealed class ScaffoldHotkeyService : IHotkeyService
{
    public bool Register(IntPtr hwnd, string hotkey, Action callback) => true;

    public void Unregister(string hotkey)
    {
    }

    public bool IsRegistered(string hotkey) => false;
}
