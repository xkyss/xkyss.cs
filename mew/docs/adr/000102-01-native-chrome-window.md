# 自绘标题栏走 DWM 原生帧扩展(NativeChromeWindow)

v0.1.1 使用系统标题栏;v0.1.2 起 Workbench 提供自绘标题栏的窗口壳「NativeChromeWindow」:采用官方样例 `NativeCustomWindow` 的 DWM 原生帧扩展(客户端内容上扩进原生标题栏区域)而非全自绘窗口,并决定 Win11-only、不做 Win10 圆角兼容。选择原生帧路线,因为它在 Direct2D + NativeAOT 目标下开销更低(无逐帧 alpha 合成),圆角/阴影/缩放/贴边由 OS 提供,与「个人自用、开发与使用环境均为 Win11」的定位匹配。

## Considered Options

- **DWM 原生帧扩展(NativeCustomWindow 路线,选定)**:`ExtendClientAreaTitleBarHeight` 把内容扩展进标题栏区域,OS 保留圆角/阴影/缩放;Win32 后端把扩展区变 HTCAPTION 获得拖拽移动;窗口按钮原生优先(`HasNativeChromeButtons` 时隐藏自绘按钮)、缺失时自绘兜底。官方注释明确建议 Win11+/macOS 优先此路。
- **全自绘窗口(CustomWindow 路线)**:`AllowsTransparency` + 自绘圆角/边框/阴影,为 Win10 及更早提供圆角观感;代价是逐帧 alpha 合成的 CPU/GPU 开销,且最大化时要手动清零圆角/边框/阴影。仅当必须兼容 Win10 时才值得。未选。

## Consequences

- 目标运行环境限定 Win11;Win10 及更早自然降级为方角,不修。
- 框架职责:Workbench 提供 `NativeChromeWindow`(标题栏左区/居中标题/右区可注入,含窗口按钮),Launcher 创建窗口并配置;关闭→托盘、`NativeMessage`、`Loaded` 等窗口级钩子留在应用侧,框架只做外壳。
- 标题栏是外壳 chrome,不属五区色板;术语见 `CONTEXT.md`「标题栏」「菜单栏」。
- 自绘关闭按钮调 `Close()` 仍走 `Closing` 事件,托盘常驻行为不变。
