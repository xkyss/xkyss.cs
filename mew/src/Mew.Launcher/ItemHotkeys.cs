using System.Runtime.InteropServices;

namespace Mew.Launcher;

/// <summary>
/// 每项热键的全局注册与分发:解析文本 → RegisterHotKey;WM_HOTKEY 按 id 分发启动;
/// 注册失败(已被占用)经回调给出可理解反馈。变更后由调用方触发 RegisterAll 重建。
/// </summary>
internal sealed class ItemHotkeys
{
    private readonly List<LauncherItem> _items;
    private readonly Action<LauncherItem> _launch;
    private readonly Action<string> _feedback;
    private readonly Dictionary<int, string> _byHotkeyId = [];
    private IntPtr _handle;
    private int _nextId = 0x1000;

    internal ItemHotkeys(List<LauncherItem> items, Action<LauncherItem> launch, Action<string> feedback)
    {
        _items = items;
        _launch = launch;
        _feedback = feedback;
    }

    internal void Attach(IntPtr windowHandle) => _handle = windowHandle;

    internal void RegisterAll()
    {
        if (_handle == IntPtr.Zero)
        {
            return;
        }

        foreach (var id in _byHotkeyId.Keys)
        {
            UnregisterHotKey(_handle, id);
        }
        _byHotkeyId.Clear();

        foreach (var item in _items)
        {
            var hotkey = item.Hotkey;
            if (string.IsNullOrWhiteSpace(hotkey))
            {
                continue;
            }

            if (!HotkeyParser.TryParse(hotkey, out var modifiers, out var vk))
            {
                continue; // 格式问题由表单实时提示
            }

            var id = _nextId++;
            if (id == GlobalHotkey.OverlayHotkeyId)
            {
                id = _nextId++; // 避开浮层热键 id
            }
            if (RegisterHotKey(_handle, id, modifiers, vk))
            {
                _byHotkeyId[id] = item.Id;
            }
            else
            {
                _feedback($"热键 {hotkey} 注册失败(可能已被占用),未生效");
            }
        }
    }

    internal bool TryLaunch(int hotkeyId)
    {
        if (!_byHotkeyId.TryGetValue(hotkeyId, out var itemId))
        {
            return false;
        }

        var item = _items.FirstOrDefault(candidate => candidate.Id == itemId);
        if (item is null)
        {
            return false;
        }

        _launch(item);
        return true;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
