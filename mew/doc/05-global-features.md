# 05 - 全局功能设计

## 概览

MewPad 需要支持应用级的跨域功能，包括**设置管理**、**多语言(i18n)**、**主题**等。
这些功能应作为**全局单例**由 `ShellContext` 持有，各组件通过观察值响应变更。

---

## 1. 主题系统

### MewUI 内置主题扩展

MewUI 提供 Light / Dark 两种基础主题。MewPad 在此基础上：

1. **系统主题跟随**（可选）
   - Windows: 跟随系统 Dark Mode 设置
   - Linux/macOS: 跟随系统主题环境变量

2. **快速切换**
   - StatusBar 右侧提供主题切换按钮
   - 点击切换 Light ↔ Dark
   - 状态持久化到 `appsettings.json`

3. **语义色板扩展**（可选）
   ```csharp
   public record AppThemePalette(
       Palette MewUI,
       Color EditorBackground,
       Color EditorForeground,
       Color EditorSelection,
       // ... 更多 VS Code 风格颜色
   );
   ```

### API 示例

```csharp
ShellContext
  .Theme.Current: Theme                    // Light / Dark
  .Theme.Changed: IObservable<Theme>       // 监听变更
  .Theme.Toggle()                          // 切换主题
  .Theme.Set(Theme theme)                  // 设置主题
```

---

## 2. 多语言系统（i18n）

### 架构

```csharp
public interface ILocalizationService
{
    /// 获取当前语言代码（如 "zh-CN", "en-US"）
    string CurrentLanguage { get; }
    IObservable<string> LanguageChanged { get; }

    /// 获取翻译文本
    string GetString(string key, string? section = null);
    string GetString(string key, params object[] args);  // 格式化

    /// 切换语言
    void SetLanguage(string languageCode);

    /// 获取可用语言列表
    IReadOnlyList<LanguageInfo> AvailableLanguages { get; }
}

public record LanguageInfo(string Code, string Name);
```

### 资源文件组织

```
Resources/
├── i18n/
│   ├── en-US.json          // 英文
│   ├── zh-CN.json          // 中文简体
│   ├── ja-JP.json          // 日文
│   └── ...
└── themes/
    ├── light.json
    └── dark.json
```

### 使用示例

```csharp
// 在 View 中
var label = new TextBlock()
    .Bind(TextBlock.TextProperty,
        () => shell.Localization.GetString("menu.file"));

// 组件初始化时响应语言变更
shell.Localization.LanguageChanged.Subscribe(_ =>
{
    UpdateUIText();  // 重新应用所有翻译
});
```

---

## 3. 设置系统

### SettingsPanel 设计

在 ActivityBar 底部固定添加 Settings 按钮，点击打开 SettingsPanel。

**SettingsPanel 结构**：

```
SettingsPanel
├── Left (SideBar)：设置分类列表
│   ├── Appearance (主题、缩放)
│   ├── Localization (语言选择)
│   ├── Editor (编辑器配置)
│   ├── Extensions (扩展管理)
│   └── About (关于应用)
│
└── Right (Content)：当前分类的设置项
    ├── [拨动开关] Theme
    ├── [下拉框] Language
    ├── [数值输入] Font Size
    └── ...
```

**SettingsPanel 接口**：

```csharp
public interface ISettingsCategory
{
    string Id { get; }
    string Title { get; }
    object Icon { get; }
    int Order { get; }
    FrameworkElement CreateView();
}

public interface ISettingsPersistence
{
    void Save(string key, object value);
    T? Load<T>(string key, T? defaultValue = null);
}
```

### 内置设置分类

| 分类 | Id | 说明 |
|------|-----|------|
| 外观 | `appearance` | 主题、缩放、字体大小 |
| 多语言 | `localization` | 语言选择、地区设置 |
| 关于 | `about` | 应用版本、许可证、致谢 |

### 扩展注册

```csharp
// 自定义扩展可注册新的设置分类
shell.Settings.RegisterCategory(new CustomSettings());
```

---

## 4. 快速切换入口

### TitleBar 右侧主题按钮

```
TitleBar 右侧（从左到右）: [🌙 Theme] [─] [□] [×]
```

- **🌙 主题按钮**：一键切换 Light ↔ Dark（立即生效并持久化）

### ActivityBar 底部设置按钮

```
ActivityBar 底部（从下往上）:
1. ⚙️ 设置 → 在 ContentArea 打开 SettingsPanel（Tab Id: `mewpad.settings`）
2. 👤 账号（预留）
3. ? 帮助（预留）
```

### 多语言设置

- 多语言切换**不提供快捷入口**
- 仅在 SettingsPanel 的 "Localization" 分类中提供下拉菜单选择
- 语言变更后，所有 UI 文本自动更新（通过 `Localization.LanguageChanged` 观察值触发）

### 实现细节

**TitleBar 主题按钮**：

```csharp
var themeButton = new Button()
    .Content("🌙")  // 或根据当前主题显示不同图标
    .ToolTip("Toggle Theme")
    .Click(() => shell.Theme.Toggle());
```

**ActivityBar 设置按钮**（在 ActivityBar 初始化时静态添加）：

```csharp
var settingsButton = new Button()
    .Content("⚙️")
    .ToolTip("Settings")
    .Click(() => OpenSettingsPanel(shell));
```

---

## 5. 应用配置文件

### appsettings.json 结构

```json
{
  "app": {
    "version": "0.1.0",
    "name": "MewPad"
  },
  "theme": {
    "current": "auto",  // "auto" | "light" | "dark"
    "followSystem": true
  },
  "localization": {
    "language": "en-US",
    "availableLanguages": ["en-US", "zh-CN", "ja-JP"]
  },
  "ui": {
    "fontSize": 12,
    "zoom": 100,
    "sideBarCollapsed": false,
    "panelCollapsed": false,
    "panelHeight": 240
  },
  "extensions": {
    "enabled": [
      "builtin.explorer",
      "builtin.search"
    ]
  }
}
```

### 持久化服务

```csharp
public interface IConfigurationService
{
    T? GetConfig<T>(string key, T? defaultValue = null);
    void SetConfig(string key, object value);
    void Save();  // 写入磁盘
}
```

---

## 6. 全局状态集成到 ShellContext

```csharp
public class ShellContext
{
    // ... 现有的 ActivityBar / Panel / Content 管理

    // 主题
    public IThemeService Theme { get; }

    // 多语言
    public ILocalizationService Localization { get; }

    // 设置
    public ISettingsService Settings { get; }

    // 配置持久化
    public IConfigurationService Configuration { get; }

    // 窗口状态（用于下次启动恢复）
    public WindowState SavedWindowState { get; set; }
}

public interface IThemeService
{
    Theme Current { get; }
    IObservable<Theme> Changed { get; }
    void Toggle();
    void Set(Theme theme);
}

public interface ISettingsService
{
    void RegisterCategory(ISettingsCategory category);
    IReadOnlyList<ISettingsCategory> Categories { get; }
    ISettingsCategory? GetCategory(string id);
    void OpenSettings(string categoryId = "appearance");
}
```

---

## 7. 初始化流程

```csharp
// Program.cs 伪代码
var config = LoadConfiguration("appsettings.json");
var shell = new ShellContext();

// 初始化主题
var themeService = new ThemeService();
themeService.Set(config.Theme.Current == "auto"
    ? DetectSystemTheme()
    : Enum.Parse<Theme>(config.Theme.Current));

// 初始化多语言
var localizationService = new LocalizationService();
localizationService.SetLanguage(config.Localization.Language);

// 注册内置设置分类
shell.Settings.RegisterCategory(new AppearanceSettings(shell.Theme));
shell.Settings.RegisterCategory(new LocalizationSettings(shell.Localization));
shell.Settings.RegisterCategory(new AboutSettings());

// 快速入口初始化（由对应组件在构造时静态添加，不通过注册系统）:
// - TitleBar 右侧: 主题切换按钮 (🌙) 由 AppWindow/TitleBarView 添加
// - ActivityBar 底部: 设置按钮 (⚙️) 由 ActivityBar 初始化时添加

// 恢复上次窗口状态
var appWindow = new AppWindow(shell);
appWindow.Width = config.UI.WindowWidth ?? 1200;
appWindow.Height = config.UI.WindowHeight ?? 800;

app.Run(appWindow);
```

---

## 8. 扩展集成

扩展可以：
- 注册新的 Settings 分类
- 订阅语言变更重新渲染 UI
- 在 StatusBar 注册自定义快速项
- 读写配置文件

```csharp
// 扩展示例：Git Panel
public class GitExtension
{
    public GitExtension(ShellContext shell)
    {
        // 监听语言变更
        shell.Localization.LanguageChanged.Subscribe(_ =>
        {
            RefreshGitUIText();
        });

        // 注册 Settings
        shell.Settings.RegisterCategory(new GitSettings());

        // 注册到 ActivityBar
        shell.RegisterActivity(new GitActivityItem());
    }
}
```
