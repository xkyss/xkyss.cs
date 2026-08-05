# Mew

个人自用的桌面 UI 框架仓库：在 MewUI 之上构建可复用的 Workbench（工作台）框架库与示例工具 App，UI 布局参考 VSCode，技术栈以 .NET 10 为主。

## Language

**Workbench**:
基于 MewUI 构建的可复用桌面应用外壳框架，提供 VSCode 式布局（活动栏、侧边栏、状态栏、中央编辑器区、底部面板等）。
_Avoid_: framework、shell、UI 框架

**MewUI**:
底层第三方 UI 框架（Aprillz.MewUI），提供控件、样式与跨平台渲染后端。本仓库在其之上构建，不 fork、不修改其源码。
_Avoid_: 底层框架、底层库

**示例工具 App**:
与 Workbench 同仓、用于自测框架能力的实际应用，是框架的验证载体。本仓库的示例工具是**应用启动管理器（Launcher）**。
_Avoid_: demo、sample

**应用启动管理器（Launcher）**:
本仓库的示例工具 App：管理并快速启动程序、脚本等启动项，支持搜索过滤与全局热键呼出。
_Avoid_: 启动器、app launcher、快捷启动器

**启动项（Launcher Item）**:
应用启动管理器管理的单个可启动条目（程序、脚本、URL 等），包含名称与启动命令，是 Launcher 的数据核心。
_Avoid_: item、条目（文档中用「启动项」）

**呼出浮层（Overlay）**:
Launcher 通过全局热键呼出的悬浮搜索窗口，独立于 Workbench 主窗口，与窗口内的管理视图共用同一份启动项数据。
_Avoid_: 弹窗、悬浮窗、popup

**活动栏（Activity Bar）**:
Workbench 左侧的纵向图标栏，用于切换侧边栏显示的面板。
_Avoid_: icon bar、导航栏

**侧边栏（Side Bar）**:
Workbench 左侧可停靠、可隐藏的辅助面板区域，承载具体工具视图（如资源管理器、搜索）。
_Avoid_: sidebar、面板（与底部面板区分）

**编辑器区（Editor Area）**:
Workbench 中央的标签化文档区域，支持停靠与拆分，基于 MewDock 的 tabset。
_Avoid_: 编辑区、中央区域

**底部面板（Panel）**:
Workbench 底部的可停靠、可折叠面板区域，用于输出、终端等辅助内容。
_Avoid_: bottom panel、底栏（与状态栏区分）

**状态栏（Status Bar）**:
Workbench 底部横条，展示当前上下文信息与操作入口。
_Avoid_: statusbar、底栏

**主题（Theme）**:
Workbench 的亮/暗外观体系，默认跟随系统并可手动切换；由 MewUI 的 ThemeManager（Seed/Metrics/Accent）与 Workbench 五区色板共同决定。
_Avoid_: 皮肤、配色方案

**五区色板（Zone Palette）**:
Workbench 定义的五区专用颜色集合（活动栏、侧边栏、编辑器区、底部面板、状态栏各自的背景与文字色），工具通过 Workbench 主题上下文读取。
_Avoid_: palette（口语可用，文档中用「五区色板」）

**标题栏（Title Bar）**:
Workbench 外壳顶部的自绘标题栏，承载应用图标、菜单栏、居中标题与右侧操作入口（含窗口按钮），替代系统标题栏；背景跟随窗口背景，不属五区色板。
_Avoid_: titlebar、系统标题栏、窗口栏

**菜单栏（Menu Bar）**:
标题栏左区内的一行菜单（如 File、Help），以访问键与快捷键触发命令。
_Avoid_: menubar、menu bar、菜单条
