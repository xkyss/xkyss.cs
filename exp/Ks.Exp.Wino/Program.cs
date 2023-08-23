using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.UI.Input.KeyboardAndMouse;

using static Windows.Win32.PInvoke;

//// 寻找指定的窗口句柄
//var h1 = PInvoke.FindWindow("Intermediate D3D Window", null);
//var h2 = PInvoke.FindWindowEx(h1, HWND.Null, "Chrome_RenderWidgetHostHWND", null);

//// 看下找到没
//Console.WriteLine($"h1: 0x{h1.Value.ToString("X8")}");
//Console.WriteLine($"h2: 0x{h2.Value.ToString("X8")}");

//// 发送滚动消息
//PInvoke.SendMessage(h2, PInvoke.WM_KEYDOWN, (WPARAM) (int) SCROLLBAR_COMMAND.SB_PAGEUP, (LPARAM)0);
//PInvoke.SendMessage(h2, PInvoke.WM_KEYUP, (WPARAM) (int) SCROLLBAR_COMMAND.SB_PAGEDOWN, (LPARAM)0);

var pos = (nint)(uint)(500 | 500 << 16);
// 左键点击消息
Console.WriteLine("1");
PostMessage((HWND)0, WM_LBUTTONDOWN, (WPARAM)(int)VIRTUAL_KEY.VK_LBUTTON, pos);
Console.WriteLine("2");
Thread.Sleep(50);
Console.WriteLine("3");
PostMessage((HWND)0, WM_LBUTTONUP, (WPARAM) (int) 0, pos);
Console.WriteLine("4");
