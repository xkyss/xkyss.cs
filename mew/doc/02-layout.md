# 02 - 整体布局设计

## 布局分区

```
┌─────────────────────────────────────────────────────────────────┐
│  TitleBar                                                        │
│  [AppIcon] [Title]  [MenuBar: File Edit View ...]  [🌙][─][□][×] │
├────┬────────────┬──────────────────────────────────────────────┤
│    │            │                                               │
│ A  │  Primary   │           ContentArea                        │
│ c  │  SideBar   │  ┌──────────────────────────────────────┐    │
│ t  │            │  │  [Tab1 ×][Tab2 ×][Tab3 ×]  [+ ▾]    │    │
│ i  │            │  ├──────────────────────────────────────┤    │
│ v  │            │  │                                      │    │
│ i  │            │  │       Content Area                   │    │
│ t  │            │  │                                      │    │
│ y  │            │  │                                      │    │
│    │            │  └──────────────────────────────────────┘    │
│ B  ├────────────┴──────────────────────────────────────────────┤
│ a  │  PanelArea                                                 │
│ r  │  [Terminal][Output][Problems]                   [─][×]     │
│    │  ┌──────────────────────────────────────────────────┐     │
│    │  │  Panel Content                                   │     │
│    │  └──────────────────────────────────────────────────┘     │
├────┴────────────────────────────────────────────────────────────┤
│ StatusBar: [Git ◉ main] [⚠ 0  ⊗ 0]  [Ln 1, Col 1]  [UTF-8]     │
└─────────────────────────────────────────────────────────────────┘
```

---

## 分区定义

| 区域 | 标识 | 默认可见 | 可折叠 |
|------|------|----------|--------|
| TitleBar | `titleBar` | ✅ | ❌ |
| ActivityBar | `activityBar` | ✅ | ❌（图标列固定存在）|
| Primary SideBar | `primarySideBar` | ✅ | ✅ |
| ContentArea | `contentArea` | ✅ | ❌（主内容区）|
| PanelArea | `panelArea` | ✅ | ✅ |
| StatusBar | `statusBar` | ✅ | ❌ |

---

## 布局实现方案

### 顶层结构（Grid 三行）

```
Grid.Rows("auto,*,auto")
├── Row 0: TitleBar
├── Row 1: MainArea（见下）
└── Row 2: StatusBar
```

### MainArea（DockPanel）

```
DockPanel
├── Left:  ActivityBar（固定宽度，如 48px）
└── Fill:  MainSplit（SplitPanel）
```

### MainSplit（SplitPanel Horizontal）

```
SplitPanel (Horizontal)
├── First:  Primary SideBar（默认 240px，可拖动，可折叠到 0）
└── Second: RightArea（见下）
```

### RightArea（SplitPanel Vertical）

```
SplitPanel (Vertical)
├── First:  ContentArea（TabControl）
└── Second: PanelArea（TabControl，可折叠到 0）
```

---

## 折叠规则

### Primary SideBar 折叠

- ActivityBar 上当前活动图标**再次点击** → 折叠 SideBar（`FirstLength = 0`，`MaxFirst = 0`）
- 图标**切换到其他项** → 展开 SideBar 并切换内容
- 折叠时保存上次宽度，展开时恢复

### PanelArea 折叠

- 点击 PanelArea 标题栏的 **[─]** 按钮 → 折叠（`SecondLength = 0`，`MaxSecond = 0`）
- 点击 ActivityBar 底部的 Panel 触发按钮，或 **View → Terminal** 等菜单 → 展开

### 尺寸约束

| 区域 | MinSize |
|------|---------|
| Primary SideBar | 120px（展开状态）|
| ContentArea | 200px |
| PanelArea | 80px（展开状态）|

---

## 交互细节

### TitleBar
- 自定义 Chrome（基于 `CustomWindow`/`NativeCustomWindow`）
- 左侧：AppIcon + 应用名称 + MenuBar
- 右侧（从左到右）：
  - **🌙 主题快速切换**：点击切换 Light ↔ Dark 主题
  - 最小化 / 最大化 / 关闭按钮
- 双击标题区域：最大化/还原
- 拖动标题区域：移动窗口

### ActivityBar
- 图标按钮垂直排列，固定宽度 48px
- **顶部区域**：主导航图标（Explorer、Search、Git 等由扩展注册）
- **底部区域**（从下往上）：
  1. **⚙️ 设置** — 打开 SettingsPanel 到 ContentArea（Tab Id: `mewpad.settings`）
  2. **👤 账号** — 用户账号/登录（预留扩展接口）
  3. **? 帮助** — 帮助 / 关于（预留扩展接口）
  4. 其他全局按钮（可由扩展注册到 Bottom 区域）
- 当前活跃项：高亮指示条（左侧 2px 竖线，Accent 色）
- 图标支持 ToolTip（显示名称）
- 底部 3 个固定项在 ActivityBar 初始化时静态创建，不通过注册系统添加

### ContentArea
- 基于 `TabControl`
- Tab 标签：图标 + 标题 + 关闭按钮（×）
- **特殊 Tab**：SettingsPanel (`mewpad.settings`)，由⚙️按钮（ActivityBar 或 StatusBar）打开，支持关闭
- 无 Tab 时显示欢迎页（Welcome Screen）
- 支持 Tab 拖动排序（后期扩展）

### PanelArea
- 基于 `TabControl`，标签在顶部
- 右侧工具栏：折叠按钮 [─]、关闭按钮 [×]
- 默认高度：240px

### StatusBar
- 固定高度 22px
- **左侧 Slot**：注册内容从左排列（Git 分支、错误数等）
- **右侧 Slot**：注册内容从右排列（扩展可注册自定义 StatusBarItem）
- 背景色：Accent 色（深色/浅色主题均用 Accent 背景 + 对比色文字）
- 各 Item 可点击（触发命令或打开面板）

---

## 窗口初始尺寸

| 属性 | 默认值 |
|------|--------|
| Width | 1200px |
| Height | 800px |
| MinWidth | 600px |
| MinHeight | 400px |
| StartupLocation | CenterScreen |
