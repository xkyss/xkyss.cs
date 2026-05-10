# 01 功能设计与数据模型

## 📊 数据模型

### LaunchItem 启动项

```csharp
/// <summary>
/// 单个启动项
/// </summary>
public class LaunchItem
{
    /// <summary>唯一标识（建议使用 kebab-case，如 github-web）</summary>
    public string Id { get; set; }

    /// <summary>显示名称（如 "GitHub"）</summary>
    public string Name { get; set; }

    /// <summary>启动项类型</summary>
    public LaunchItemType Type { get; set; }

    /// <summary>目标内容</summary>
    /// <remarks>
    /// 根据 Type：
    /// - Web: URL (https://github.com)
    /// - Application: 程序路径 (C:\Program Files\...\Code.exe)
    /// - Script: 命令行命令 (npm list)
    /// </remarks>
    public string Target { get; set; }

    /// <summary>所属一级分类 ID</summary>
    public string Category { get; set; }

    /// <summary>可选：二级分类 ID（暂时保留用于扩展）</summary>
    public string? SubCategory { get; set; }

    /// <summary>描述信息</summary>
    public string? Description { get; set; }

    /// <summary>图标（emoji 或 emoji 组合）</summary>
    public string? Icon { get; set; }

    /// <summary>排序优先级（值越小越靠前）</summary>
    public int Order { get; set; }

    /// <summary>是否启用</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>最后修改时间</summary>
    public DateTime? ModifiedAt { get; set; }
}

public enum LaunchItemType
{
    /// <summary>网页链接</summary>
    Web = 0,

    /// <summary>本地应用程序</summary>
    Application = 1,

    /// <summary>脚本/命令</summary>
    Script = 2
}
```

### LaunchCategory 分类

```csharp
/// <summary>
/// 一级分类
/// </summary>
public class LaunchCategory
{
    /// <summary>分类 ID（建议使用 kebab-case，如 dev-tools）</summary>
    public string Id { get; set; }

    /// <summary>分类名称（如 "开发工具"）</summary>
    public string Name { get; set; }

    /// <summary>分类图标（emoji）</summary>
    public string? Icon { get; set; }

    /// <summary>排序优先级（值越小越靠前）</summary>
    public int Order { get; set; }

    /// <summary>是否启用</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>最后修改时间</summary>
    public DateTime? ModifiedAt { get; set; }
}
```

### LaunchConfig 全局配置

```csharp
/// <summary>
/// QuickLaunch 完整配置
/// </summary>
public class LaunchConfig
{
    /// <summary>格式版本（用于向后兼容）</summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>所有分类</summary>
    public List<LaunchCategory> Categories { get; set; } = new();

    /// <summary>所有启动项</summary>
    public List<LaunchItem> Items { get; set; } = new();

    /// <summary>用户偏好设置</summary>
    public LaunchPreferences? Preferences { get; set; }
}

public class LaunchPreferences
{
    /// <summary>默认打开的分类 ID</summary>
    public string? DefaultCategoryId { get; set; }

    /// <summary>是否启用调试模式</summary>
    public bool DebugEnabled { get; set; } = false;

    /// <summary>最近使用历史（项目 ID 列表）</summary>
    public List<string> RecentlyUsed { get; set; } = new();
}
```

---

## 🎯 核心功能

### 1. 启动项执行

#### Web 类型
```csharp
// 使用系统默认浏览器打开 URL
Process.Start(new ProcessStartInfo
{
    FileName = item.Target,
    UseShellExecute = true
});
```

#### Application 类型
```csharp
// 启动本地应用程序
Process.Start(item.Target);
```

#### Script 类型
```csharp
// 在 cmd 或 PowerShell 中执行脚本
Process.Start(new ProcessStartInfo
{
    FileName = "cmd.exe",
    Arguments = $"/c {item.Target}",
    CreateNoWindow = false
});
```

### 2. 配置管理

#### 加载配置
- 启动时从 `{ConfigDir}/quicklaunch-config.json` 读取
- 若文件不存在，加载内置默认配置
- 配置解析失败时，使用回退方案

#### 保存配置
- 用户在 Settings 修改分类/项目后，立即保存
- 每次保存前验证数据完整性
- 保存成功后通知 UI 刷新

### 3. 分类管理

| 操作 | 实现 |
|------|------|
| **列出分类** | LaunchService.GetCategories() |
| **新增分类** | LaunchService.AddCategory() |
| **修改分类** | LaunchService.UpdateCategory() |
| **删除分类** | LaunchService.DeleteCategory()（删除时需处理关联项目） |
| **排序分类** | LaunchService.SortCategories() |

### 4. 启动项管理

| 操作 | 实现 |
|------|------|
| **按分类查询** | LaunchService.GetItemsByCategory(categoryId) |
| **新增项目** | LaunchService.AddItem() |
| **修改项目** | LaunchService.UpdateItem() |
| **删除项目** | LaunchService.DeleteItem() |
| **执行启动** | LaunchService.Launch(itemId) |
| **搜索** | LaunchService.Search(query) |

---

## 🏗️ 模块职责

### LaunchService 业务逻辑层

```csharp
public interface ILaunchService
{
    // 分类操作
    Task<List<LaunchCategory>> GetCategoriesAsync();
    Task<LaunchCategory?> GetCategoryAsync(string categoryId);
    Task AddCategoryAsync(LaunchCategory category);
    Task UpdateCategoryAsync(LaunchCategory category);
    Task DeleteCategoryAsync(string categoryId);

    // 启动项操作
    Task<List<LaunchItem>> GetItemsAsync();
    Task<List<LaunchItem>> GetItemsByCategoryAsync(string categoryId);
    Task<LaunchItem?> GetItemAsync(string itemId);
    Task AddItemAsync(LaunchItem item);
    Task UpdateItemAsync(LaunchItem item);
    Task DeleteItemAsync(string itemId);

    // 搜索
    Task<List<LaunchItem>> SearchAsync(string query);

    // 执行
    Task LaunchAsync(string itemId);

    // 导入导出
    Task<string> ExportConfigAsync();
    Task ImportConfigAsync(string json);
}
```

### 各 UI 组件职责

| 组件 | 职责 |
|------|------|
| **QuickLaunchActivity** | 显示分类列表，响应分类选择事件 |
| **QuickLaunchPanel** | 显示当前分类的启动项列表，提供快速启动 |
| **QuickLaunchContent** | 显示详细视图、搜索框、编辑操作 |
| **QuickLaunchSettings** | 分类/项目的 CRUD 管理页面 |

---

## 💾 配置存储位置

```
Windows:
  %APPDATA%/MewPad/quicklaunch-config.json

Linux:
  ~/.config/mewpad/quicklaunch-config.json

macOS:
  ~/Library/Application Support/MewPad/quicklaunch-config.json
```

---

## 🔗 关键集成点

### 与 MewPad 核心的集成

1. **ISettingsService** - 保存配置到 MewPad settings
2. **ILocalizationService** - 多语言支持（分类名、项目名等）
3. **IThemeService** - 主题感知 UI
4. **ShellContext** - 注册 Activity、Panel、Settings、StatusBar
5. **ObservableValue<T>** - 响应式状态管理（当前分类）

