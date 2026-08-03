# Workbench 需求

可复用的桌面应用外壳框架,提供 VSCode 式布局。

## 布局

- **五区布局**:活动栏、侧边栏、编辑器区、底部面板、状态栏,术语见 `CONTEXT.md`。
- 编辑器区基于 **MewDock** 的 tabset,支持停靠与拆分。
- Workbench 提供**类型化五区布局 API**(编译期组合,Fluent),不向使用者暴露 MewDock 原始模型。
- 原生标题栏。

## 主题

- 亮/暗两套外观,默认跟随系统,应用内可手动切换。
- **五区色板**(活动栏/侧边栏/编辑器区/底部面板/状态栏的背景与文字色)收敛在 Workbench,工具通过 Workbench 主题上下文读取。
- 强调色默认 `Accent.Blue`。

## 布局保存/恢复

- 布局序列化为 JSON,保存到 `%APPDATA%\Mew\layout.json`。
- 启动时恢复上次布局;保存时机由 Workbench 管理(布局变更后/退出前)。
