# MewPad 代码质量改进计划（2026-05-11）

> 基于对项目代码的全面审查，梳理出以下待改进项，按优先级和分类组织。

---

## 当前代码质量状态

| 维度 | 评级 | 说明 |
|------|------|------|
| 编译状态 | ✅ 通过 | 0 错误，2 个可忽略警告 |
| 测试通过率 | ✅ 45/45 | 全部通过但部分测试有效性不足 |
| 架构清晰度 | ⚠️ 中等 | 核心文件职责过重，缺少分层 |
| 异常处理 | 🔴 薄弱 | 多处静默吞异常，无日志抽象 |
| 测试覆盖率 | ⚠️ 不足 | 缺少 UI 组件和关键路径测试 |

---

## P0 紧急：数据安全与测试可信度

### 1. ConfigurationService 异常吞没

**现状**：`src/MewPad.Core/Services/Impl/ConfigurationService.cs` 第 27 行和第 53 行使用 `catch { }` 静默吞掉所有异常。

**风险**：配置文件损坏或磁盘不可写时，数据静默丢失，用户无感知。

**改进方案**：
```csharp
// 方案一：引入 Debug 日志（最小改动）
catch (Exception ex)
{
    Debug.WriteLine($"[ConfigurationService] Load failed: {ex.Message}");
}

// 方案二：引入 ILogger 抽象（推荐中长期）
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to load configuration from {Path}", _configPath);
}
```

**验收标准**：配置读写异常时至少有调试输出，不静默吞掉。

**文件**：`src/MewPad.Core/Services/Impl/ConfigurationService.cs`

---

### 2. 修复无效的配置持久化测试

**现状**：`tests/MewPad.Tests/ConfigurationServiceTests.cs`

- `Save_PersistsConfigToFile`（第 72 行）：只断言 `Assert.NotNull(service)`，未验证文件内容。
- `MultipleInstances_ShareSameConfig`（第 118 行）：只断言 `Assert.NotNull(service2)`，未验证数据可读。

**改进方案**：
```csharp
[Fact]
public void Save_PersistsConfigToFile()
{
    var service = new ConfigurationService();
    var testKey = "persist-key-" + Guid.NewGuid();

    service.SetConfig(testKey, "persist-value");
    service.Save();

    // 创建新实例读取验证
    var service2 = new ConfigurationService();
    Assert.Equal("persist-value", service2.GetConfig<string>(testKey));
}

[Fact]
public void MultipleInstances_ShareSameConfig()
{
    var testKey = "multi-instance-test-" + Guid.NewGuid();

    var service1 = new ConfigurationService();
    service1.SetConfig(testKey, "shared-value");
    service1.Save();

    var service2 = new ConfigurationService();
    Assert.Equal("shared-value", service2.GetConfig<string>(testKey));
}
```

**验收标准**：测试真正验证配置持久化和跨实例共享行为。

**文件**：`tests/MewPad.Tests/ConfigurationServiceTests.cs`

---

### 3. CloseContent 回退逻辑改进

**现状**：`src/MewPad.Core/Shell/ShellContext.cs` 第 117-119 行：
```csharp
ActiveContentId.Value = _contentItems.Keys.FirstOrDefault();
```

**问题**：关闭当前激活 Tab 时总是跳到剩余 Tab 中的第一个，不符合 VS Code 最近使用逻辑。

**改进方案**：记录 Tab 打开历史，关闭时切换到最近使用的 Tab。
```csharp
// 在 ShellContext 中新增
private readonly List<string> _contentOpenHistory = [];

// OpenContent 中记录历史
if (!_contentItems.ContainsKey(item.Id))
{
    _contentItems[item.Id] = item;
}
_contentOpenHistory.Remove(item.Id);
_contentOpenHistory.Add(item.Id);
ActiveContentId.Value = item.Id;

// CloseContent 中使用历史回退
public void CloseContent(string id)
{
    _contentItems.Remove(id);
    _contentOpenHistory.Remove(id);

    if (ActiveContentId.Value == id)
    {
        var lastActive = _contentOpenHistory.LastOrDefault();
        ActiveContentId.Value = lastActive ?? _contentItems.Keys.FirstOrDefault();
    }
}
```

**验收标准**：关闭当前 Tab 后自动切换到最近使用的 Tab。

**文件**：`src/MewPad.Core/Shell/ShellContext.cs`

---

## P1 重要：架构拆分与可靠性

### 4. AppWindowBuilder 拆分

**现状**：`src/MewPad.Hosting/AppWindowBuilder.cs` 超过 876 行，承担窗口构建、主题管理、状态栏、ActivityBar、SideBar、ContentArea、PanelArea、菜单栏、快捷键、设置面板、状态持久化等全部职责。

**拆分方案**：

```
src/MewPad.Hosting/
├── AppWindowBuilder.cs              // 主入口，Window 创建和根布局组装
├── Builders/
│   ├── ActivityBarBuilder.cs        // ActivityBar + 导航指示器
│   ├── ContentAreaManager.cs        // Tab 管理、内容切换逻辑
│   ├── PanelAreaBuilder.cs          // 底部面板区域
│   ├── StatusBarBuilder.cs          // 状态栏组装
│   └── SettingsOverlayManager.cs    // 设置面板显示/隐藏逻辑
├── Extensions/
│   ├── AppearanceSettings.cs        （不动）
│   ├── LanguageSettings.cs          （不动）
│   ├── AboutSettings.cs             （不动）
│   └── SettingsContentItem.cs       （不动）
├── Infrastructure/
│   └── NativeCustomWindow.cs        （不动）
├── Plugins/
│   ├── PluginConfig.cs              （不动）
│   └── PluginLoader.cs              （不动）
└── Program.cs                       （精简，职责委托给各 Builder）
```

**关键原则**：
- 每个 Builder 不超过 300 行
- 通过 ShellContext 进行通信，不直接跨模块引用
- Builder 方法按调用顺序排列，保持 `BuildShell` 可读性

**验收标准**：`AppWindowBuilder.cs` 缩减至 200 行以内，逻辑拆分到各子文件中。

**文件**：`src/MewPad.Hosting/AppWindowBuilder.cs`

---

### 5. ClosableTabControl 事件泄漏

**现状**：`src/MewPad.Core/Components/ClosableTabControl/ClosableTabControl.cs` 第 136-154 行，`BindTabHoverEvents()` 每次重建时全量遍历 VisualTree 并绑定事件，但**不解绑旧事件**。

**风险**：多次添加/删除 Tab 后，事件重复绑定导致内存泄漏或重复触发。

**改进方案**：
```csharp
// 方案一：改用 Style Trigger 控制可见性（最推荐，与现有模式一致）
// 在 TabHeaderButton 的 Style 中添加 Hover 触发器控制关闭按钮可见性
// 这样完全不需要手动绑定事件

// 方案二：如果必须代码绑定，先解绑再绑
private readonly Dictionary<UIElement, (MouseEnterEventHandler, MouseLeaveEventHandler)> _hoverBindings = new();

private void BindTabHoverEvents()
{
    // 先解绑所有旧事件
    foreach (var (element, handlers) in _hoverBindings)
    {
        element.MouseEnter -= handlers.Item1;
        element.MouseLeave -= handlers.Item2;
    }
    _hoverBindings.Clear();

    var index = 0;
    VisualTree.Visit(Inner, el =>
    {
        if (el.GetType().Name == "TabHeaderButton" && el is UIElement thb)
        {
            if (index >= _closeBtns.Count) return;
            var btn = _closeBtns[index++];
            if (btn == null) return;

            void OnEnter(object? s, MouseEventArgs e) => btn.WithTheme((t, b) =>
                b.Foreground = t.Palette.WindowText);
            void OnLeave(object? s, MouseEventArgs e) => btn.WithTheme((t, b) =>
                b.Foreground = Color.FromRgb(0, 0, 0).WithAlpha(0));

            thb.MouseEnter += OnEnter;
            thb.MouseLeave += OnLeave;
            _hoverBindings[thb] = (OnEnter, OnLeave);
        }
    });
}
```

**验收标准**：多次 AddTab/RemoveTab 后无内存泄漏，关闭按钮 hover 正常。

**文件**：`src/MewPad.Core/Components/ClosableTabControl/ClosableTabControl.cs`

---

### 6. 引入日志抽象

**现状**：调试信息散落在各处使用 `Debug.WriteLine`：
- `src/MewPad.Hosting/Plugins/PluginLoader.cs` — 无日志
- `src/MewPad.Hosting/Program.cs` — 使用 `Console.WriteLine`
- `samples/QuickLaunch.Plugin/Services/LaunchService.cs` — 使用 `Debug.WriteLine`

**改进方案**：
```csharp
// 1. 定义日志接口（在 MewPad.Core 中）
public interface ILogger
{
    void LogDebug(string message);
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? exception = null);
}

// 2. 在 ShellContext 中注册
public ILogger Logger { get; }

// 3. 各模块注入使用
public class ConfigurationService : IConfigurationService
{
    private readonly ILogger? _logger;
    public ConfigurationService(ILogger? logger = null) => _logger = logger;

    private void LoadConfiguration()
    {
        try { ... }
        catch (Exception ex)
        {
            _logger?.LogError($"Failed to load config: {ex.Message}");
        }
    }
}
```

**验收标准**：所有模块日志通过统一接口输出，可替换为正式日志库（如 Serilog）。

**文件**：`src/MewPad.Core/Services/ILogger.cs`（新建）

---

### 7. 插件生命周期管理

**现状**：`IPlugin` 接口仅有 `Register` 方法，缺少初始化、卸载、依赖声明。

**改进方案**：
```csharp
public interface IPlugin : IDisposable
{
    string Id { get; }

    /// <summary>最低宿主版本要求</summary>
    string MinHostVersion { get; }

    /// <summary>依赖的其他插件 ID</summary>
    string[] Dependencies { get; }

    /// <summary>注册到 Shell</summary>
    void Register(ShellContext shell);

    /// <summary>异步初始化（可选资源加载等）</summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>卸载时调用</summary>
    void Shutdown();
}

// PluginLoader 中补充生命周期调用
public static PluginLoadSummary LoadFromDirectory(ShellContext shell, string pluginsDirectory, PluginLoadOptions? options = null)
{
    // ... 已有加载逻辑 ...

    // 注册后调用 InitializeAsync
    if (plugin is IInitializablePlugin initPlugin)
    {
        await initPlugin.InitializeAsync();
    }
}

// 卸载时
public static async Task UnloadAsync(ShellContext shell, string pluginId)
{
    // 调用 Shutdown
    // 从 ShellContext 移除已注册项
}
```

> **注意**：此改进需要与 `PluginLoader` 的 `RegisterFromAssembly` 协同修改。考虑到当前插件均为进程内加载，`Shutdown` 可先作为预留接口。

**验收标准**：`IPlugin` 支持生命周期方法，`PluginLoader` 正确调用。

**文件**：`src/MewPad.Core/Plugins/IPlugin.cs`、`src/MewPad.Hosting/Plugins/PluginLoader.cs`

---

## P2 建议：代码规范与健壮性

### 8. 魔法字符串/数字提取为常量

**现状**：代码中散见魔法值：

| 文件 | 行 | 魔法值 |
|------|----|--------|
| `AppWindowBuilder.cs` | 19 | `0x00, 0x7A, 0xCC`（AccentColor） |
| `AppWindowBuilder.cs` | 358 | `"mewpad.settings"` |
| `AppWindowBuilder.cs` | 48 | `48`（ActivityBar 宽度） |
| `NativeCustomWindow.cs` | 14 | `28`（TitleBar 高度） |
| `NativeCustomWindow.cs` | 16 | `32`（按钮宽度） |
| `NativeCustomWindow.cs` | 17 | `4`（ChromeButtonSize） |
| `PluginLoader.cs` | 186 | `"0.1.0-preview"`（HostVersion） |

**改进方案**：提取到各文件的 `static class Constants` 或统一 `Constants.cs`。

```csharp
// AppWindowBuilder.cs 或独立文件
internal static class ShellConstants
{
    public const string SettingsContentId = "mewpad.settings";
    public const double ActivityBarWidth = 48;
    public static readonly Color AccentColor = Color.FromRgb(0x00, 0x7A, 0xCC);
}
```

**验收标准**：所有魔法值替换为具名常量，消除硬编码。

---

### 9. openSettingsAction 闭包模式替换

**现状**：`AppWindowBuilder.cs` 第 85-86 行：
```csharp
Action<string> openSettingsAction = _ => { };
```
随后在第 347 行被重新赋值。这种模式绕过编译器对未初始化变量的检查。

**改进方案**：改用事件或直接调用方法：
```csharp
// 替换为方法
private void OpenSettings(string categoryId = "appearance")
{
    // ... 原 openSettingsAction 逻辑 ...
}

// SettingsContentItem.cs 也通过 ShellContext 事件触发
shell.Settings.SettingsOpening += (sender, categoryId) => OpenSettings(categoryId);
```

**验收标准**：消除 `openSettingsAction` 闭包赋值模式，改为方法调用或事件机制。

---

### 10. IContentItem.CreateContent() UI 缓存

**现状**：`QuickLaunchContent.CreateContent()` 每次调用都创建全新 UI 元素。如果内容被反复打开/关闭，会导致 GC 压力和状态丢失。

**改进方案**：
```csharp
// IContentItem 中添加可选接口
public interface IRecyclableContentItem : IContentItem
{
    /// <summary>是否缓存已创建的 UI</summary>
    bool CacheContent { get; } = true;
}

// ContentAreaManager 中检查是否已缓存
if (item is IRecyclableContentItem recyclable && recyclable.CacheContent)
{
    if (_contentCache.TryGetValue(item.Id, out var cached))
        return cached;
}
```

**验收标准**：可缓存的内容项不重复创建 UI 元素。

**文件**：`src/MewPad.Core/Interfaces/IContentItem.cs`、`src/MewPad.Hosting/AppWindowBuilder.cs`

---

### 11. Card 属性设置批处理优化

**现状**：`Card.cs` 中每次设置 `Title`、`Extra`、`Body` 等属性都独立调用 `Rebuild()`，触发 `InvalidateMeasure()` + `InvalidateVisual()`。

**改进方案**：引入批量更新模式：
```csharp
public class Card : Control, IVisualTreeHost
{
    private bool _isBatchUpdating;

    public IDisposable BeginBatchUpdate()
    {
        _isBatchUpdating = true;
        return new BatchDisposable(() =>
        {
            _isBatchUpdating = false;
            Rebuild();
        });
    }

    private void Rebuild()
    {
        _layoutRoot.Clear();
        // ... 原有逻辑 ...

        if (!_isBatchUpdating)
        {
            InvalidateMeasure();
            InvalidateVisual();
        }
    }

    private sealed class BatchDisposable : IDisposable
    {
        private readonly Action _onDispose;
        public BatchDisposable(Action onDispose) => _onDispose = onDispose;
        public void Dispose() => _onDispose?.Invoke();
    }
}
```

**验收标准**：使用 `using (card.BeginBatchUpdate())` 批量设置属性时只触发一次重排。

**文件**：`src/MewPad.Core/Components/Card/Card.cs`

---

## P3 建议：测试完善

### 12. 提取共享 Mock 类

**现状**：`MockActivityItem` 在 `ShellContextTests.cs:156` 和 `AppStartupIntegrationTests.cs:127` 重复定义。

**改进方案**：创建 `tests/MewPad.Tests/TestUtils/MockActivityItem.cs` 和 `MockSettingsCategory.cs`，供所有测试文件引用。

**文件**：`tests/MewPad.Tests/`

---

### 13. 补充关键测试

| 待测项 | 文件路径 | 优先级 |
|--------|---------|--------|
| `ClosableTabControl` 添加/删除/关闭/悬停行为 | `tests/MewPad.Tests/`（新建 `ClosableTabControlTests.cs`） | 高 |
| `Card` 各属性组合渲染 | `tests/MewPad.Tests/`（新建 `CardTests.cs`） | 高 |
| `ShellContext.CloseContent` 回退逻辑 | `tests/MewPad.Tests/ShellContextTests.cs` | 高 |
| `PluginManifest.IsHostVersionCompatible` 边界测试 | `tests/MewPad.Tests/PluginLoaderTests.cs` | 中 |
| `LocalizationService` 未找到 key 回退 | `tests/MewPad.Tests/`（新建 `LocalizationTests.cs`） | 中 |

**验收标准**：核心组件和业务逻辑的测试覆盖率达到 80% 以上。

---

## 改进路线图

| 阶段 | 编号 | 改进项 | 预估工时 |
|------|------|--------|---------|
| P0 | #1 | ConfigurationService 异常处理 | 0.5h |
| P0 | #2 | 修复无效的持久化测试 | 0.5h |
| P0 | #3 | CloseContent 回退逻辑 | 0.5h |
| P1 | #4 | AppWindowBuilder 拆分 | 2-3h |
| P1 | #5 | ClosableTabControl 事件泄漏修复 | 1h |
| P1 | #6 | 日志抽象引入 | 1h |
| P1 | #7 | 插件生命周期扩展 | 1h |
| P2 | #8 | 魔法值提取 | 0.5h |
| P2 | #9 | openSettingsAction 闭包替换 | 0.5h |
| P2 | #10 | CreateContent UI 缓存 | 0.5h |
| P2 | #11 | Card 批量更新优化 | 0.5h |
| P3 | #12 | Mock 类提取 | 0.5h |
| P3 | #13 | 补充关键测试 | 2-3h |

**合计预估**：约 12-14 小时

---

## 执行建议

1. **先 P0 后 P1 再 P2**：确保数据安全和测试可信度后再做架构拆分
2. **每次提交对应一个编号**：便于 Code Review 追踪
3. **P0 完成后立即运行全部测试**：确保改进不引入回归
4. **拆分 AppWindowBuilder（#4）时保持功能不变**：只拆分文件结构，不修改逻辑

---

*本文档由 AI 辅助生成，2026-05-11*