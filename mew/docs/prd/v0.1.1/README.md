# v0.1.1 产品需求(PRD)

- **对应 ADR**:`000101-*`(见 `docs/adr/000101-01-lock-mewui-version.md` 等)
- **术语**:见仓库根 `CONTEXT.md`

## 目标

构建个人自用的桌面 UI 框架:一个可复用的 **Workbench(工作台)框架库** 与同仓的 **应用启动管理器(Launcher)** 示例工具,布局参考 VSCode,技术栈以 .NET 10 为主。

## 范围(v0.1.1)

- Workbench:五区布局 + 亮暗主题 + 布局保存/恢复 + 编译期组合 API
- Launcher:窗口内管理 + 全局热键呼出浮层 + 托盘常驻 + 启动项管理
- 发布:NativeAOT 单文件 exe(Windows 10+ / Direct2D)

详见 [workbench.md](workbench.md)、[launcher.md](launcher.md)、[non-functional.md](non-functional.md)。

## 范围外(留待 v1.1+)

- 命令面板、设置页
- 开机自启开关
- 多平台(Linux/macOS)后端

## 验收标准(概览)

1. Workbench 能呈现五区布局,支持亮/暗主题切换与布局保存恢复。
2. Launcher 能管理启动项(增删改查 + 搜索),热键呼出浮层并快速启动。
3. 关闭主窗口后托盘常驻,热键仍可用。
4. 可按 NativeAOT 配置发布为单文件 exe。
