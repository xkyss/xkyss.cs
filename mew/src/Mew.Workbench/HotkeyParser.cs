namespace Mew.Workbench;

/// <summary>
/// 每项热键文本(Ctrl+Shift+1 形式)解析为 Win32 修饰键与虚拟键;至少要求一个修饰键,避免裸键劫持普通输入。
/// </summary>
public static class HotkeyParser
{
    public const uint ModAlt = 0x1;
    public const uint ModControl = 0x2;
    public const uint ModShift = 0x4;
    public const uint ModWin = 0x8;

    public static bool TryParse(string text, out uint modifiers, out uint vk)
    {
        modifiers = 0;
        vk = 0;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var parts = text.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            return false;
        }

        for (var i = 0; i < parts.Length - 1; i++)
        {
            switch (parts[i].ToLowerInvariant())
            {
                case "ctrl" or "control":
                    modifiers |= ModControl;
                    break;
                case "alt":
                    modifiers |= ModAlt;
                    break;
                case "shift":
                    modifiers |= ModShift;
                    break;
                case "win" or "windows" or "meta":
                    modifiers |= ModWin;
                    break;
                default:
                    return false;
            }
        }

        return TryMapKey(parts[^1].ToLowerInvariant(), out vk);
    }

    private static bool TryMapKey(string name, out uint vk)
    {
        vk = 0;
        if (name.Length == 1)
        {
            var c = name[0];
            if (c is >= 'a' and <= 'z')
            {
                vk = (uint)(c - 'a' + 0x41);
                return true;
            }
            if (c is >= '0' and <= '9')
            {
                vk = (uint)(c - '0' + 0x30);
                return true;
            }
            return false;
        }

        if (name[0] == 'f' && int.TryParse(name[1..], out var f) && f is >= 1 and <= 24)
        {
            vk = (uint)(0x70 + f - 1);
            return true;
        }

        vk = name switch
        {
            "space" => 0x20,
            "enter" or "return" => 0x0D,
            "escape" or "esc" => 0x1B,
            "tab" => 0x09,
            "up" => 0x26,
            "down" => 0x28,
            "left" => 0x25,
            "right" => 0x27,
            "home" => 0x24,
            "end" => 0x23,
            "pageup" => 0x21,
            "pagedown" => 0x22,
            "delete" or "del" => 0x2E,
            "insert" or "ins" => 0x2D,
            "backspace" => 0x08,
            "printscreen" => 0x2C,
            "pause" => 0x13,
            _ => 0,
        };
        return vk != 0;
    }
}
