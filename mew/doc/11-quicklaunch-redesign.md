# QuickLaunch 插件重新设计

## 📌 目标

打造一个轻量级的**快速启动工具**，支持快速打开：
- 🌐 网页
- 💻 本地软件
- 🔧 脚本/其他任务

支持**二级分类导航**，提升大量快捷项的可用性。

---

## 🏗️ 功能设计

### 1. 数据模型

#### LaunchItem（启动项）
```csharp
public class LaunchItem
{
    public string Id { get; set; }                  // 唯一标识
    public string Name { get; set; }                // 显示名称
    public LaunchItemType Type { get; set; }        // 类型：Web / App / Script
    public string Target { get; set; }              // 目标：URL / 程序路径 / 脚本路径
    public string? Description { get; set; }        // 描述
    public string? Icon { get; set; }               // 图标（emoji或路径）
    public string? Category { get; set; }           // 分类（一级）
    public string? SubCategory { get; set; }        // 子分类（二级）
    public int Order { get; set; }                  // 排序
    public bool Enabled { get; set; } = true;       // 是否启用
}

public enum LaunchItemType
{
    Web,        // 网页链接
    Application, // 本地应用
    Script      // 脚本/命令
}
```

#### LaunchCategory（分类）
```csharp
public class LaunchCategory
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Icon { get; set; }
    public int Order { get; set; }
}
```

#### 配置结构
```json
{
  "categories": [
    { "id": "work", "name": "工作", "icon": "💼", "order": 1 },
    { "id": "dev", "name": "开发", "icon": "🔨", "order": 2 },
    { "id": "entertainment", "name": "娱乐", "icon": "🎮", "order": 3 }
  ],
  "items": [
    {
      "id": "github",
      "name": "GitHub",
      "type": "Web",
      "target": "https://github.com",
      "category": "dev",
      "icon": "🐙",
      "order": 1
    },
    {
      "id": "vscode",
      "name": "Visual Studio Code",
      "type": "Application",
      "target": "C:\\Program Files\\Microsoft VS Code\\Code.exe",
      "category": "dev",
      "icon": "⚙️",
      "order": 2
    }
  ]
}
```

---

## 🎨 界面设计

### 布局架构

```
┌─────────────────────────────────────────────────┐
│  MewPad 主窗口                                   │
├─────────────────────────────────────────────────┤
│ ActivityBar │ SideBar          │ Content Area    │
│             │                  │                 │
│   ⚡        │ 📂 工作          │ 【工作】        │
│  Quick      │ 📝 打开文档      │ ├─ 文档编辑器  │
│  Launch     │ 🔗 打开网页      │ ├─ 浏览器     │
│             │ 💻 启动软件      │ ├─ 邮件       │
│             │ 🎮 娱乐          │                 │
│             │ 🔧 开发          │                 │
│             │ ⚙️  设置 (内置)  │                 │
└─────────────────────────────────────────────────┘
```

### 2.1 Activity 面板（左侧快捷入口）

**位置**: ActivityBar 左侧
**展示内容**:
- 显示所有一级分类
- 点击分类切换 SideBar 内容
- 显示一级分类的快捷项计数

**UI 示例**:
```
┌──────────────┐
│ ⚡ QuickLaunch│
├──────────────┤
│ 💼 工作  (5)  │
│ 🔨 开发  (8)  │
│ 🎮 娱乐  (3)  │
│ 🔧 其他  (2)  │
│              │
│ ⚙️  设置      │
└──────────────┘
```

**功能**:
- 显示当前选中分类（高亮）
- 一键切换到对应分类
- 设置按钮进入 Settings

---

### 2.2 SideBar 面板（二级分类和快捷项）

**位置**: MewPad 右侧 Panel 区域
**展示内容**:
- 选中分类的所有二级项
- 分组显示（可选，若有子分类）
- 每个快捷项显示：icon + name + 简要描述

**UI 示例**（选中"开发"时）:
```
┌─────────────────────────────┐
│ 🔨 开发                      │
├─────────────────────────────┤
│ 🐙 GitHub                   │
│   版本控制平台              │
├─────────────────────────────┤
│ ⚙️ VS Code                  │
│   代码编辑器                │
├─────────────────────────────┤
│ 📦 npm                      │
│   Node 包管理器             │
├─────────────────────────────┤
│ ➕ 添加新项目               │
└─────────────────────────────┘
```

**交互**:
- 点击快捷项 → 执行启动（打开网页/启动软件/运行脚本）
- 右键 / 长按 → 编辑 / 删除 / 复制快捷方式
- "添加新项目" → 跳转到 Settings 对应分类

---

### 2.3 内容区（Content Area）

**位置**: MewPad 中央主内容区
**展示内容**:
- 当前分类的详细视图（可选）
- 所有快捷项的完整列表 + 描述
- 搜索框（过滤快捷项）
- 统计信息（该分类有多少项）

**UI 示例**（选中"开发"时）:
```
┌───────────────────────────────────────┐
│ 🔨 开发工具                            │
├───────────────────────────────────────┤
│ 🔍 [搜索快捷项...]                     │
├───────────────────────────────────────┤
│ 共 8 个快捷项                         │
├───────────────────────────────────────┤
│ ┌─────────────────────────────────┐   │
│ │ 🐙 GitHub                       │   │
│ │    https://github.com           │   │
│ │    [启动]  [编辑]  [删除]       │   │
│ └─────────────────────────────────┘   │
│                                       │
│ ┌─────────────────────────────────┐   │
│ │ ⚙️ Visual Studio Code           │   │
│ │    C:\Program Files\...\Code.exe│   │
│ │    [启动]  [编辑]  [删除]       │   │
│ └─────────────────────────────────┘   │
│                                       │
│ ... 更多项 ...                        │
└───────────────────────────────────────┘
```

---

### 2.4 Settings 配置页

**位置**: 通过 Activity 中的 ⚙️ 按钮进入
**内容**:
- 分类管理（新增、编辑、删除、排序）
- 快捷项管理（新增、编辑、删除、排序）
- 导入/导出配置
- 高级选项（启用调试日志、清缓存等）

**分类管理表格**:
```
┌──────────────────────────────────────┐
│ 分类管理                               │
├──────────────────────────────────────┤
│ 名称      │ Icon  │ 项数  │ 操作      │
├──────────────────────────────────────┤
│ 工作      │ 💼    │  5   │ 编辑 删除 │
│ 开发      │ 🔨    │  8   │ 编辑 删除 │
│ 娱乐      │ 🎮    │  3   │ 编辑 删除 │
├──────────────────────────────────────┤
│ [+ 新增分类]                          │
└──────────────────────────────────────┘
```

**快捷项管理表格**:
```
┌────────────────────────────────────────────────┐
│ 快捷项管理（分类：开发）                         │
├────────────────────────────────────────────────┤
│ 名称     │ 类型   │ 目标               │ 操作    │
├────────────────────────────────────────────────┤
│ GitHub   │ Web    │ https://github.com │ 编辑 删除│
│ VS Code  │ App    │ C:\...\Code.exe   │ 编辑 删除│
│ npm      │ Script │ npm list           │ 编辑 删除│
├────────────────────────────────────────────────┤
│ [+ 新增快捷项]                                  │
└────────────────────────────────────────────────┘
```

---

## 🔄 交互流程

### 用户场景 1: 快速启动网页

```
用户点击 Activity 中的 "💼 工作"
    ↓
SideBar 显示工作分类的所有快捷项
    ↓
Content Area 显示该分类的详细视图
    ↓
用户点击 SideBar 中的 "🌐 打开文档"
    ↓
系统执行 → 浏览器打开对应 URL
```

### 用户场景 2: 新增快捷项

```
用户点击 Activity 中的 "⚙️ 设置"
    ↓
进入 Settings 页面
    ↓
用户选择分类（如 "开发"）
    ↓
用户点击 "[+ 新增快捷项]"
    ↓
弹出编辑框：输入 Name / Type / Target / Icon / Description
    ↓
用户点击 "保存"
    ↓
配置自动保存到 settings.json
    ↓
返回 Activity，新项目立即可用
```

---

## 📁 数据存储

### 配置文件位置

```
<MewPad 配置目录>/quicklaunch-config.json
```

### 默认配置示例

参考 [示例配置](#) 或见下文

---

## 🛠️ 技术实现要点

### 1. 模块划分

| 模块 | 职责 | 类 |
|------|------|-----|
| **Model** | 数据模型 | LaunchItem, LaunchCategory |
| **Service** | 业务逻辑 | LaunchService（加载、保存、执行） |
| **UI - Activity** | Activity 面板 | QuickLaunchActivity |
| **UI - Panel** | SideBar 面板 | QuickLaunchPanel |
| **UI - Content** | 内容页面 | QuickLaunchContent |
| **UI - Settings** | 配置页面 | QuickLaunchSettings |

### 2. 启动执行

- **Web 类型**: 调用 `Process.Start(target)` 用系统默认浏览器打开
- **Application 类型**: `Process.Start(target)` 启动程序
- **Script 类型**: `Process.Start("cmd.exe", $"/c {target}")` 或 PowerShell

### 3. 配置持久化

- 使用 `ISettingsService` 的 `GetSettings<T>` / `SetSettings<T>` 方法
- QuickLaunch 在 Plugin Settings 下注册一个 "Launcher Configuration" 分类
- 配置实时保存到 MewPad.Hosting 的 settings.json

### 4. 状态管理

- 使用 `ObservableValue<T>` 管理当前选中分类
- Activity 绑定 ObservableValue，分类选择时自动刷新 SideBar
- SideBar 和 Content Area 监听同一个 ObservableValue

---

## 📊 项目结构

```
samples/QuickLaunch.Plugin/
├── QuickLaunch.Plugin.csproj
├── QuickLaunchPluginEntrypoint.cs
├── Services/
│   └── LaunchService.cs
├── Models/
│   ├── LaunchItem.cs
│   └── LaunchCategory.cs
├── UI/
│   ├── QuickLaunchActivity.cs
│   ├── QuickLaunchPanel.cs
│   ├── QuickLaunchContent.cs
│   └── QuickLaunchSettings.cs
├── Resources/
│   └── default-config.json
└── README.md
```

---

## 📋 开发清单（阶段）

### Phase 1: 核心功能（MVP）
- [ ] 数据模型（LaunchItem, LaunchCategory）
- [ ] LaunchService（加载、保存、执行）
- [ ] Activity 面板（分类列表）
- [ ] SideBar 面板（快捷项列表）
- [ ] 默认配置文件 + 示例项目

### Phase 2: 增强功能
- [ ] Content Area（详细视图）
- [ ] Settings 管理页面（CRUD）
- [ ] 搜索/过滤
- [ ] 导入/导出配置

### Phase 3: 高级功能
- [ ] 快捷键支持
- [ ] 拖拽排序
- [ ] 最近使用统计
- [ ] 多行编辑器（长脚本）

---

## ✅ 验收标准

1. ✓ 能读写配置文件，配置持久化
2. ✓ Activity 显示所有分类，可切换
3. ✓ SideBar 根据选中分类动态显示快捷项
4. ✓ 点击快捷项能正确启动网页/软件/脚本
5. ✓ Settings 可新增/编辑/删除分类和快捷项
6. ✓ 默认配置包含 5-10 个示例项

---

## 🎯 下一步

1. **确认**上述设计是否满足需求？
2. **调整**任何需要改进的地方
3. **确定**开发优先级和阶段
4. **开始编码**

