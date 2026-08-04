using System.Runtime.InteropServices;

namespace Mew.Launcher;

/// <summary>
/// 全局热键(Ctrl+Alt+Space)注册:基于 Win32 RegisterHotKey,经窗口 NativeMessage 消息钩子接收。
/// </summary>
internal static class GlobalHotkey
{
    private const int ModAlt = 0x1;
    private const int ModControl = 0x2;
    private const int ModNoRepeat = 0x4000;
    private const int VkSpace = 0x20;
    private const int Id = 0x4D57; // "MW"

    /// <summary>WM_HOTKEY</summary>
    internal const uint WmHotkey = 0x0312;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    internal static bool Register(IntPtr hWnd) =>
        RegisterHotKey(hWnd, Id, ModControl | ModAlt | ModNoRepeat, VkSpace);

    internal static void Unregister(IntPtr hWnd) => UnregisterHotKey(hWnd, Id);
}
