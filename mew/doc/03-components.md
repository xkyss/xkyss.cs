# 03 - 组件设计

## 概览

MewPad 的每个布局区域对应一个 Shell 组件类。所有组件：

- 继承自 MewUI 的 `UserControl`
- 通过 `WithTheme` 绑定颜色，不硬编码颜色值
- 对外暴露简洁的 C# API

---

## 1. AppWindow（主窗口）

```csharp
public class AppWindow : NativeCustomWindow  // 或 CustomWindow（Win10）
```

**职责**：顶层窗口宿主，持有所有分区的引用，提供 `ShellContext`。

**属性**：

| 属性 | 类型 | 说明 |
|------|------|------|
| `Shell` | `ShellContext` | 全局 Shell 状态与注册中心 |

**布局构建**（OnBuild 伪代码）：

```csharp
Grid.Rows("auto,*,auto")
  .Children(
    new TitleBarView(Shell),          // Row 0
    new MainAreaView(Shell),          // Row 1
    new StatusBarView(Shell)          // Row 2
  )
```

---

## 2. TitleBarView

**职责**：自定义标题栏，含应用图标、标题、菜单栏、窗口控制按钮。

**结构**：

```
DockPanel
├── Left:  [AppIcon 16px] [AppTitle TextBlock]
├── Center(Fill): [MenuBar] — DragMove 区域
└── Right: [MinBtn][MaxBtn][CloseBtn]
```

**关键 API**：

```csharp
TitleBarView
  .Title(string title)
  .Icon(ImageSource icon)
  .AddMenu(MenuModel menu)          // 添加顶级菜单
```

**注意**：标题栏中的非按钮区域注册 `DragMove`，双击触发 `MaximizeRestore`。

---

## 3. ActivityBarView

**职责**：最左侧图标导航栏，承载 `IActivityItem` 列表，控制 SideBar 内容切换与折叠。

**结构**：

```
DockPanel (Vertical, Width=48)
├── Fill: TopItems (StackPanel, Vertical)  ← 注册的导航图标
└── Bottom: BottomItems (StackPanel)       ← 固定：Settings 等
```

**单个图标按钮**：

```
Button (48×48, no border)
  Icon (Glyph/Image, 22px)
  ToolTip = item.Title
  左侧 2px 竖条 = 活跃指示（仅活跃项可见）
```

**状态**：

- `ActiveItemId: string?` — 当前激活项 ID
- 再次点击已激活项 → 切换 `SideBarCollapsed`

---

## 4. SideBarView

**职责**：主侧边栏容器，顶部显示当前视图标题，内容区展示 `IActivityItem.CreateContent()`。

**结构**：

```
Grid.Rows("auto,*")
├── Row 0: Header — [Title TextBlock] [... 工具按钮]
└── Row 1: ContentPresenter — 当前 IActivityItem 内容
```

**折叠实现**：由父级 `MainAreaView` 控制 `SplitPanel.MaxFirst`，SideBarView 本身不控制可见性。

---

## 5. MainAreaView

**职责**：组合 ActivityBar + SideBar + EditorArea + PanelArea 的布局容器。

**结构**（伪代码）：

```csharp
DockPanel
  .Children(
    new ActivityBarView(Shell).DockLeft(),
    new MainSplit(Shell)               // Fill
  )

MainSplit = SplitPanel(Horizontal)
  .First(new SideBarView(Shell))
  .Second(new RightSplit(Shell))
  .MinFirst(0)        // 允许完全折叠
  .MinSecond(200)

RightSplit = SplitPanel(Vertical)
  .First(new ContentAreaView(Shell))
  .Second(new PanelAreaView(Shell))
  .MinFirst(100)
  .MinSecond(0)       // 允许完全折叠
```

**折叠状态管理**：

- `SideBarCollapsed: bool` → `MainSplit.MaxFirst = collapsed ? 0 : double.PositiveInfinity`
- `PanelCollapsed: bool` → `RightSplit.MaxSecond = collapsed ? 0 : double.PositiveInfinity`
- 展开时恢复上次 `FirstLength`/`SecondLength`（GridLength.Pixel）

---

## 6. ContentAreaView

**职责**：中変 Tab 式内容区域，管理 `IContentItem` 列表。

**结构**：

```
Grid.Rows("auto,*")
├── Row 0: TabStrip — [Tab1 ×][Tab2 ×]...[+ 新建]
└── Row 1: ContentPresenter — 当前 Tab 的 Element
```

**空状态**：无 Tab 时展示 WelcomeView（可由注册者替换）。

**Tab 项**（`ContentTab`）：

```
StackPanel (Horizontal)
  [Icon 14px] [Title TextBlock] [CloseButton ×]
```

**关键 API**：

```csharp
ContentAreaView
  .OpenTab(IContentItem item)      // 打开或激活已有同 id Tab
  .CloseTab(string id)
  .ActiveTabId: string?
```

---

## 7. PanelAreaView

**职责**：底部面板区，Tab 式，右侧含折叠/关闭控制。

**结构**：

```
Grid.Rows("auto,*")
├── Row 0: Header
│     DockPanel
│       Fill: TabStrip（[Terminal][Output][Problems]...）
│       Right: [CollapseBtn ─] [CloseBtn ×]
└── Row 1: ContentPresenter
```

**关键 API**：

```csharp
PanelAreaView
  .AddPanel(IPanelItem item)
  .ActivePanelId: string?
  .Collapse()
  .Expand()
```

---

## 8. StatusBarView

**职责**：底部状态栏，Slot 机制左右两区注册 Item。

**结构**：

```
DockPanel (Height=22, Background=Accent)
  Fill: LeftSlots (StackPanel, Horizontal)
  Right: RightSlots (StackPanel, Horizontal, reversed)
```

**StatusBarItem**：

```csharp
public record StatusBarItem(
    string Id,
    Func<FrameworkElement> CreateElement,
    StatusBarSlot Slot,      // Left / Right
    int Priority             // 排序，数值小靠外
);
```

---

## 9. ShellContext（核心状态容器）

**职责**：全局单例，持有所有注册列表与可观察状态。

```csharp
public class ShellContext
{
    // === ActivityBar / Panel / Content 管理 ===
    public void RegisterActivity(IActivityItem item);
    public void RegisterPanel(IPanelItem item);
    public void RegisterStatusBarItem(StatusBarItem item);

    // === 内容区管理 ===
    public void OpenContent(IContentItem item);      // 打开或激活 Content Tab
    public void CloseContent(string id);             // 关闭 Content Tab
    public ObservableValue<string?> ActiveContentId { get; }

    // === 布局状态 ===
    public ObservableValue<string?> ActiveActivityId { get; }
    public ObservableValue<bool> SideBarCollapsed { get; }
    public ObservableValue<bool> PanelCollapsed { get; }
    public ObservableValue<string?> ActivePanelId { get; }

    // === 全局功能服务 ===
    public IThemeService Theme { get; }              // 主题管理
    public ILocalizationService Localization { get; }// 多语言
    public ISettingsService Settings { get; }        // 设置分类
    public IConfigurationService Configuration { get; } // 配置持久化
}
```
