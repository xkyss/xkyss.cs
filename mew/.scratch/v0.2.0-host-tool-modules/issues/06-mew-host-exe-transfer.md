# 06 — Mew.Host 建立与 exe 角色移交

**What to build:** 新宿主 exe 成为唯一启动入口:平台注册、组合根显式注册模块、独占 Build()、窗口/托盘/标题栏/菜单/关于/主题循环/设置文档(外观节)全部归宿主;Launcher 工程由 WinExe 转为 Library,发布配置(应用图标/NativeAOT/Direct2D)移入宿主。产物变为 `Mew.Host.exe` + `Mew.Launcher.dll`。用户视角:启动入口换为宿主,其余行为与 v0.1.6 一致。

**Blocked by:** 05

**Status:** ready-for-agent

- [ ] Mew.Host.exe 启动即 Launcher 应用:五区、托盘、全局热键、浮层、设置文档(外观节)行为与 v0.1.6 一致
- [ ] Launcher 转 Library 后,既有领域测试仍全绿
- [ ] NativeAOT 发布(win-x64)仍可行,产物为 Mew.Host.exe;窗口标题/托盘提示/关于保留产品名「Mew Launcher」
- [ ] 设置文档由宿主提供,外观(主题)节归宿主;Launcher 仅贡献「数据」节
