using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Ks.Exp.Wino;
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

// 左键点击消息
{
	var pos = (nint)(uint)(100 | 90 << 16);
	var hwnd = (HWND)0x1808EA;
	SendMessage(hwnd, WM_LBUTTONDOWN, (WPARAM)0, pos);
	Thread.Sleep(50);
	SendMessage(hwnd, WM_LBUTTONUP, (WPARAM)0, pos);
}





var size1 = Marshal.SizeOf(typeof(BaseStruct));
Console.WriteLine($"BaseStruct Size={size1}");

var size2 = Marshal.SizeOf(typeof(SubStruct));
Console.WriteLine($"SubStruct Size={size2}");

var ret = new BaseStruct();
unsafe
{
    var p1 = &ret;
    var p2 = &(ret.Field1);
}