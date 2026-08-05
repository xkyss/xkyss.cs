using System.Runtime.InteropServices;

namespace Mew.Launcher;

/// <summary>
/// 浮层全局热键注册:基于 Win32 RegisterHotKey,经窗口 NativeMessage 消息钩子接收。
/// 组合键可配置(默认 Ctrl+Alt+Space),变更后重新注册即时生效。
/// </summary>
internal static class GlobalHotkey
{
    private const uint ModNoRepeat = 0x4000;
    private const int OverlayHotkeyIdConst = 0x4D57; // "MW"

    /// <summary>浮层热键注册 id</summary>
    internal const int OverlayHotkeyId = OverlayHotkeyIdConst;

    /// <summary>WM_HOTKEY</summary>
    internal const uint WmHotkey = 0x0312;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    /// <summary>
    /// 按文本组合键(如 Ctrl+Alt+Space)注册浮层热键;解析失败或注册失败(已被占用)返回 false。
    /// </summary>
    internal static bool Register(IntPtr hWnd, string hotkey)
    {
        if (!HotkeyParser.TryParse(hotkey, out var modifiers, out var vk))
        {
            return false;
        }

        return RegisterHotKey(hWnd, OverlayHotkeyId, modifiers | ModNoRepeat, vk);
    }

    internal static void Unregister(IntPtr hWnd) => UnregisterHotKey(hWnd, OverlayHotkeyId);
}
