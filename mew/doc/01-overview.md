# MewPad 设计总览

## 项目定位

**MewPad** 是一个基于 [MewUI](https://github.com/aprillz/MewUI) 和 .NET 10 构建的通用桌面 GUI Shell 框架。

- 参考 VS Code 的分区布局，提供统一的宿主窗口骨架
- 以"插件化 View 注册"为核心扩展机制，业务代码只需实现接口即可嵌入
- 跨平台：Windows / Linux (X11) / macOS（依赖 MewUI 的平台后端）
- 目标运行时：.NET 10，支持 NativeAOT

---

## 文档索引

| 文档 | 说明 |
|------|------|
| [01-overview.md](01-overview.md) | 项目定位与总览（本文件）|
| [02-layout.md](02-layout.md) | 整体布局分区与交互规则 |
| [03-components.md](03-components.md) | 各分区的组件设计与 API |
| [04-extensibility.md](04-extensibility.md) | 插件化扩展机制设计 |
| [05-global-features.md](05-global-features.md) | 主题、多语言、设置等全局功能 |
| [06-development-plan.md](06-development-plan.md) | 详细开发计划与里程碑 |
| [07-quick-start.md](07-quick-start.md) | 项目初始化与编码快速指南 |

---

## 技术栈

| 层 | 选型 |
|----|------|
| UI 框架 | Aprillz.MewUI（NuGet: `Aprillz.MewUI`）|
| 运行时 | .NET 10 |
| 语言 | C# 13，代码优先（无 XAML）|
| 主题 | MewUI 内置 Light / Dark 自动跟随系统 |
| AOT | 可选，与 MewUI NativeAOT Ready 对齐 |

---

## 设计原则

1. **Shell 与内容分离**：Shell 只负责区域的显示/隐藏/切换，不耦合业务逻辑。
2. **注册即接入**：ActivityBar 图标、SideBar 面板、ContentArea Tab、PanelArea Tab 都通过统一注册接口添加。
3. **最小接口**：扩展者只需实现 `IView`（提供图标 + 标题 + 内容 Element），其余由 Shell 管理。
4. **布局可折叠**：SideBar / PanelArea 均可展开/收起，折叠状态持久化到应用设置。
5. **主题透明**：所有 Shell 组件使用 MewUI `WithTheme` 绑定色板，自动响应 Light/Dark 切换。
