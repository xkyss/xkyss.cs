using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

// 寻找指定的窗口句柄
var h1 = PInvoke.FindWindow("Notepad", null);
var h2 = PInvoke.FindWindowEx(h1, HWND.Null, "Edit", null);

// 看下找到没
Console.WriteLine($"h1: 0x{h1.Value.ToString("X8")}");
Console.WriteLine($"h2: 0x{h2.Value.ToString("X8")}");

// 发送滚动消息
PInvoke.SendMessage(h2, PInvoke.WM_VSCROLL, (WPARAM) (int) SCROLLBAR_COMMAND.SB_PAGEUP, (LPARAM)0);
PInvoke.SendMessage(h2, PInvoke.WM_VSCROLL, (WPARAM) (int) SCROLLBAR_COMMAND.SB_PAGEDOWN, (LPARAM)0);
