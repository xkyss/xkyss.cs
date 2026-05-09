# 04 - 可扩展性设计

## 核心理念

MewPad 采用**注册式插件模型**：Shell 不感知具体业务，所有内容通过接口注册到 `ShellContext`。
扩展者只需：

1. 实现对应接口（`IActivityItem` / `IPanelItem` / `IEditorItem`）
2. 在应用启动时调用 `shell.Register*(...)` 注入

---

## 扩展接口定义

### IActivityItem — ActivityBar + SideBar 内容

```csharp
public interface IActivityItem
{
    /// 唯一标识，用于激活/切换
    string Id { get; }

    /// ActivityBar 显示的图标（Glyph 字符或 ImageSource）
    object Icon { get; }

    /// SideBar 标题栏文字
    string Title { get; }

    /// ActivityBar 中的位置
    ActivityBarSection Section => ActivityBarSection.Top;

    /// 排序权重，数值小靠上
    int Order => 100;

    /// 创建 SideBar 内容区域（每次激活时调用，或缓存由 Shell 决定）
    FrameworkElement CreateContent();
}

public enum ActivityBarSection { Top, Bottom }
```

### IPanelItem — PanelArea 底部面板

```csharp
public interface IPanelItem
{
    string Id { get; }
    string Title { get; }
    object? Icon { get; }       // 可选
    int Order => 100;

    /// 创建面板内容（首次显示时调用，之后缓存）
    FrameworkElement CreateContent();
}
```

### IContentItem — ContentArea Tab 内容

```csharp
public interface IContentItem
{
    /// Tab 唯一标识（相同 Id 复用已有 Tab）
    string Id { get; }

    string Title { get; }
    object? Icon { get; }

    /// 是否允许关闭（false = 固定 Tab，无 × 按钮）
    bool CanClose => true;

    FrameworkElement CreateContent();
}
```

### StatusBarItem — 状态栏插槽

```csharp
public record StatusBarItem(
    string Id,
    Func<FrameworkElement> CreateElement,
    StatusBarSlot Slot = StatusBarSlot.Left,
    int Priority = 100
);

public enum StatusBarSlot { Left, Right }
```

---

## 内容缓存策略

| 接口 | 缓存策略 | 说明 |
|------|----------|------|
| `IActivityItem` | **每次激活重用**，首次创建后缓存实例 | SideBar 切换时不销毁，仅切换可见性 |
| `IPanelItem` | **首次激活后缓存** | 面板内容保持状态（如终端会话）|
| `IContentItem` | **按 Id 缓存** | 相同 Id 不重复创建，`OpenContent` 仅激活 Tab |

---

## 启动注册示例

```csharp
// Program.cs
var app = new Application();
var shell = new ShellContext();

// 注册 ActivityBar 视图
shell.RegisterActivity(new ExplorerView());   // 文件树
shell.RegisterActivity(new SearchView());     // 搜索
shell.RegisterActivity(new GitView());        // Git

// 注册底部面板
shell.RegisterPanel(new TerminalPanel());
shell.RegisterPanel(new OutputPanel());

// 注册状态栏
shell.RegisterStatusBarItem(new StatusBarItem(
    "git-branch",
    () => new TextBlock().Text("⎇ main"),
    StatusBarSlot.Left,
    Priority: 10
));

// 默认打开的 Content Tab（可选）
shell.OpenContent(new WelcomeContentItem());

app.Run(new AppWindow(shell));
```

---

## 内置扩展项（Shell 自带）

Shell 本身会预注册以下最小集合，保证"开箱即用"：

| 类型 | Id | 说明 |
|------|----|------|
| `IActivityItem` | — | 无预置，由宿主应用注册 |
| `IPanelItem` | — | 无预置 |
| `IContentItem` | `mewpad.welcome` | 欢迎页（无 Tab 时显示）|
| `IContentItem` | `mewpad.settings` | 设置面板（由 ActivityBar ⚙️ 或菜单打开）|
| `StatusBarItem` | `mewpad.position` | 行列号（右侧）|
| `StatusBarItem` | `mewpad.encoding` | 编码（右侧）|

---

## 命令系统（后期扩展，暂设计占位）

为支持菜单与快捷键，预留命令注册接口：

```csharp
// 暂不实现，设计占位
public interface ICommand
{
    string Id { get; }
    string Title { get; }
    Action Execute { get; }
    Func<bool>? CanExecute { get; }
    KeyGesture? DefaultKeybinding { get; }
}

// shell.RegisterCommand(ICommand cmd);
// MenuBar 项可绑定 CommandId
```

---

## 扩展点汇总

```
ShellContext
├── RegisterActivity(IActivityItem)    → ActivityBar + SideBar
├── RegisterPanel(IPanelItem)          → PanelArea Tab
├── RegisterStatusBarItem(...)         → StatusBar 左/右 Slot
├── OpenContent(IContentItem)          → ContentArea Tab
└── [预留] RegisterCommand(ICommand)   → 菜单/快捷键
```
