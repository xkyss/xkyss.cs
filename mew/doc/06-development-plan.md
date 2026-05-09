# MewPad 开发计划

## 项目概览

**项目名称**：MewPad
**运行时**：.NET 10
**基础框架**：MewUI
**目标**：通用桌面 GUI Shell 框架
**开发周期**：分阶段迭代（MVP → Feature Complete → Polish）

---

## 项目结构

```
MewPad/
├── src/
│   ├── MewPad.Core/                    # 核心库（Shell 框架）
│   │   ├── Shell/
│   │   │   ├── ShellContext.cs         # 全局单例
│   │   │   ├── ActivityBar/            # 导航栏
│   │   │   ├── SideBar/                # 侧边栏
│   │   │   ├── ContentArea/            # 内容区
│   │   │   ├── PanelArea/              # 底部面板
│   │   │   └── StatusBar/              # 状态栏
│   │   ├── Services/
│   │   │   ├── IThemeService.cs
│   │   │   ├── ILocalizationService.cs
│   │   │   ├── ISettingsService.cs
│   │   │   └── IConfigurationService.cs
│   │   ├── Interfaces/
│   │   │   ├── IActivityItem.cs
│   │   │   ├── IPanelItem.cs
│   │   │   ├── IContentItem.cs
│   │   │   └── ISettingsCategory.cs
│   │   └── Extensions/
│   │       └── ... (fluent API helpers)
│   │
│   ├── MewPad.Hosting/                 # 应用宿主
│   │   ├── Program.cs                  # 入口点
│   │   ├── AppWindow.cs                # 主窗口
│   │   ├── TitleBarView.cs
│   │   ├── AppSettings/
│   │   │   ├── AppearanceSettings.cs
│   │   │   ├── LocalizationSettings.cs
│   │   │   └── AboutSettings.cs
│   │   ├── Resources/
│   │   │   ├── i18n/
│   │   │   │   ├── en-US.json
│   │   │   │   └── zh-CN.json
│   │   │   └── appsettings.json
│   │   └── MewPad.Hosting.csproj
│   │
│   └── MewPad.Hosting.csproj           # 或合并为单一项目
│
├── doc/                                 # 设计文档
│   ├── 01-overview.md
│   ├── 02-layout.md
│   ├── 03-components.md
│   ├── 04-extensibility.md
│   ├── 05-global-features.md
│   └── DEVELOPMENT.md (本文件)
│
├── MewPad.sln / MewPad.slnx
└── README.md
```

---

## 开发阶段

### Phase 1: 核心框架与基础 UI（Weeks 1-2）

**目标**：实现 Shell 骨架和基本布局，支持主题切换。

#### 任务

- [ ] **项目结构初始化**
  - [ ] 创建 MewPad.sln 及 Core / Hosting 项目
  - [ ] 配置项目文件：TargetFramework=net10.0, OutputType=WinExe, NativeAOT 选项
  - [ ] 添加 MewUI NuGet 依赖

- [ ] **核心接口定义** (`MewPad.Core/Interfaces/`)
  - [ ] `IActivityItem` — ActivityBar 导航项接口
  - [ ] `IPanelItem` — PanelArea 面板接口
  - [ ] `IContentItem` — ContentArea Tab 接口
  - [ ] `ISettingsCategory` — 设置分类接口

- [ ] **全局服务接口** (`MewPad.Core/Services/`)
  - [ ] `IThemeService` 及 `Theme` enum
  - [ ] `ILocalizationService`
  - [ ] `ISettingsService`
  - [ ] `IConfigurationService`

- [ ] **ShellContext 实现**
  - [ ] 单例管理（ActivityBar 列表 / Panel 列表 / Content Tab 列表）
  - [ ] 状态属性：`ActiveActivityId`、`SideBarCollapsed`、`PanelCollapsed` 等（`ObservableValue<T>` 或 INotifyPropertyChanged）
  - [ ] 内容管理 API：`OpenContent()`、`CloseContent()`
  - [ ] 全局服务属性：`Theme`、`Localization`、`Settings`、`Configuration`

- [ ] **主窗口 AppWindow**
  - [ ] 继承 `NativeCustomWindow` 或 `CustomWindow`
  - [ ] 三层 Grid：TitleBar / MainArea / StatusBar
  - [ ] TitleBar 包含菜单栏 + 主题切换按钮（🌙）
  - [ ] 接收 `ShellContext`，传递给各子视图

- [ ] **ActivityBar 视图**
  - [ ] 垂直按钮栏，固定宽度 48px
  - [ ] 绑定 ShellContext 的 Activity 列表
  - [ ] 活跃指示条（左侧竖线）
  - [ ] 底部固定项：⚙️ Settings、👤 Account（预留）、? Help（预留）
  - [ ] 点击项切换 SideBar 内容

- [ ] **SideBar 视图**
  - [ ] 标题栏 + 内容区
  - [ ] 绑定当前活跃 Activity 的内容
  - [ ] 折叠/展开逻辑（可拖动分割线）

- [ ] **ContentArea 视图**
  - [ ] TabControl 基础
  - [ ] 无 Tab 时显示欢迎页

- [ ] **PanelArea 视图**
  - [ ] 底部可折叠面板（TabControl）
  - [ ] 默认包含 Terminal / Output 占位 Tab

- [ ] **StatusBar 视图**
  - [ ] 左右两侧插槽
  - [ ] 基础 Git 分支 + 错误数示例项

- [ ] **主题系统实现**
  - [ ] `ThemeService` 实现
  - [ ] Light / Dark 切换逻辑
  - [ ] MewUI 色板应用（`WithTheme` 绑定）
  - [ ] 配置持久化到 `appsettings.json`

- [ ] **样本扩展**（验证注册机制）
  - [ ] 创建简单 `ExplorerActivity` （文件树占位）
  - [ ] 在 Program.cs 中注册并测试

---

### Phase 2: 多语言与设置系统（Weeks 3-4）

**目标**：实现 i18n、Settings Panel、配置管理。

#### 任务

- [ ] **多语言系统**
  - [ ] `LocalizationService` 实现
  - [ ] 资源文件加载（JSON）
  - [ ] 语言切换接口 + 事件通知
  - [ ] 初始化时检测系统语言或读取配置

- [ ] **资源文件准备**
  - [ ] `en-US.json` — 英文资源
  - [ ] `zh-CN.json` — 中文资源
  - [ ] 关键文本条目：菜单、标签、工具提示

- [ ] **设置面板实现**
  - [ ] `SettingsPanel` 主视图（TwoPane 布局）
  - [ ] 设置分类列表（左）+ 分类内容（右）
  - [ ] Tab 项：可由 ⚙️ 按钮打开/关闭

- [ ] **内置设置分类**
  - [ ] `AppearanceSettings` — 主题、缩放、字体大小
  - [ ] `LocalizationSettings` — 语言选择
  - [ ] `AboutSettings` — 版本、许可证、致谢
  - [ ] 各分类对应的 View

- [ ] **配置持久化**
  - [ ] `ConfigurationService` 实现
  - [ ] `appsettings.json` 读写
  - [ ] 窗口状态保存（尺寸、位置、折叠状态）

- [ ] **UI 动态应用多语言**
  - [ ] 各视图组件订阅 `LanguageChanged` 事件
  - [ ] 更新文本内容逻辑

---

### Phase 3: 高级交互与状态管理（Weeks 5-6）

**目标**：完善折叠/展开交互，实现状态持久化与恢复。

#### 任务

- [ ] **SideBar 折叠交互**
  - [ ] 当前项再次点击 → 折叠 SideBar
  - [ ] 点击不同项 → 展开并切换内容
  - [ ] 保存上次宽度，恢复时应用

- [ ] **PanelArea 折叠交互**
  - [ ] [─] 按钮折叠、[×] 按钮关闭
  - [ ] Panel 触发按钮展开

- [ ] **状态恢复逻辑**
  - [ ] 应用启动时恢复窗口尺寸/位置
  - [ ] 恢复上次打开的 Activity / Panel
  - [ ] 恢复主题、语言、各区域折叠状态

- [ ] **ContentArea Tab 管理**
  - [ ] 打开 / 关闭 Tab
  - [ ] Tab 拖动排序（后期，可选）
  - [ ] 欢迎页显示（无 Tab）

- [ ] **菜单栏集成**
  - [ ] File / Edit / View / Help 菜单
  - [ ] View 菜单项：Toggle SideBar / Toggle Panel 等

---

### Phase 4: 测试与文档（Weeks 7-8）

**目标**：单元测试、集成测试、文档完善。

#### 任务

- [ ] **单元测试**
  - [ ] ShellContext 状态管理测试
  - [ ] Activity / Panel / Content 注册机制测试
  - [ ] 配置服务测试

- [ ] **集成测试**
  - [ ] 完整启动流程测试
  - [ ] 主题切换响应测试
  - [ ] 语言切换 UI 更新测试

- [ ] **示例应用与文档**
  - [ ] 创建 Sample 项目演示扩展开发
  - [ ] 编写扩展开发指南（README）
  - [ ] 更新项目 README

- [ ] **性能与优化**
  - [ ] NativeAOT 兼容性检查
  - [ ] 启动时间测试
  - [ ] 内存使用分析

---

## MVP 功能清单

### 必须实现

- ✅ 窗口布局（TitleBar / ActivityBar / SideBar / ContentArea / PanelArea / StatusBar）
- ✅ 注册机制（IActivityItem / IPanelItem / IContentItem）
- ✅ ShellContext 全局单例
- ✅ 主题切换（Light / Dark），TitleBar 快速按钮
- ✅ 多语言支持（i18n，至少英 / 中）
- ✅ 设置面板（Settings）
- ✅ ActivityBar 设置按钮（⚙️）

### 可选（Post-MVP）

- 🔲 Tab 拖动排序
- 🔲 扩展市场 / 插件加载机制
- 🔲 快捷键系统
- 🔲 命令面板
- 🔲 键盘导航优化

---

## 技术决策

### 状态管理

**选项**：
- 方案 A：`INotifyPropertyChanged` + 事件
- 方案 B：`ObservableValue<T>` (自定义或使用 Reactive Extensions)

**决定**：采用 **ObservableValue<T>** (自定义，简单易控制) 或 **INotifyPropertyChanged** (内置支持)。
推荐后者，MewUI 原生支持属性绑定。

### 配置格式

**选项**：JSON / YAML / XML
**决定**：**JSON**（轻量、易解析，与 .NET 配置系统兼容）

### i18n 实现

**选项**：
- .NET 内置 ResourceManager（.resx）
- 自定义 JSON 加载
- 第三方库（如 Localization.Resources）

**决定**：**JSON + 手工加载**（简单、易扩展、减少依赖）

### 窗口 Chrome

**选项**：
- `CustomWindow` (Win10 风格，AllowsTransparency，性能差)
- `NativeCustomWindow` (Win11+ 原生支持)

**决定**：优先 **NativeCustomWindow**（Win11/macOS），回退 `CustomWindow`（Win10）

---

## 技术栈与依赖

| 组件 | 版本 | 说明 |
|------|------|------|
| .NET | 10.0 | 目标框架 |
| MewUI | Latest | UI 框架 |
| System.Text.Json | 内置 | JSON 序列化 |
| System.Resources | 内置 | 资源管理 |
| ReactiveExtensions | 可选 | 状态管理（可选） |

---

## 每周里程碑

| 周 | 目标 | 交付物 |
|----|------|--------|
| Week 1 | 项目结构 + 接口定义 + ShellContext | 编译通过，框架骨架就位 |
| Week 2 | UI 视图布局 + 主题系统 | 可显示主题切换功能 |
| Week 3 | 多语言系统 + 资源文件 | 语言切换生效 |
| Week 4 | 设置面板 + 配置持久化 | 设置可保存恢复 |
| Week 5 | 交互完善 + 状态恢复 | 完整用户体验 |
| Week 6 | 菜单栏 + 高级功能 | 接近完整功能 |
| Week 7 | 测试 + 文档 | 可交付版本 |
| Week 8 | 优化 + 示例 | Release v0.1.0 |

---

## 编码规范

- **命名**：PascalCase (类、接口、公开方法)，camelCase (私有字段、参数)
- **注释**：关键逻辑添加 XML 文档注释，复杂算法添加行注释
- **错误处理**：公开 API 捕获异常并记录；内部异常可向上传播
- **依赖注入**：在 Program.cs 中集中配置，ShellContext 作为单例容器

---

## 分支策略

```
main (release)
  └─ develop (integration)
      ├─ feature/core-framework
      ├─ feature/ui-layout
      ├─ feature/theme-system
      ├─ feature/i18n
      ├─ feature/settings
      └─ ...
```

---

## 关键检查点

### Before Phase 2
- [ ] 代码编译无警告
- [ ] 基础 UI 按设计显示
- [ ] ActivityBar 与 SideBar 交互正常
- [ ] 主题切换无闪烁

### Before Phase 3
- [ ] 多语言系统运行无异常
- [ ] 设置面板能打开/关闭
- [ ] 配置文件成功读写

### Before Phase 4
- [ ] 所有 MVP 功能可用
- [ ] 启动、退出流程正常
- [ ] 没有明显的性能问题

---

## 风险与缓解

| 风险 | 影响 | 缓解策略 |
|------|------|---------|
| MewUI 文档不足 | 开发效率低 | 提前阅读源码示例，建立代码片段库 |
| 跨平台兼容性 | 延期 | 前期专注 Windows，后期测试 Linux/macOS |
| NativeAOT 编译 | 无法交付 | 保持代码 AOT-friendly，early testing |
| 状态管理复杂 | Bug 难调 | 单元测试先行，分离 UI 与业务逻辑 |

---

## 后续扩展（不在 MVP 范围）

1. **扩展系统**：动态加载 .dll 插件
2. **命令面板**：快速搜索 + 执行
3. **快捷键绑定**：可配置快捷键
4. **主题市场**：用户自定义主题
5. **工作区概念**：保存 / 恢复工作环境
6. **拖放支持**：文件拖入编辑区等

---

## 相关资源

- [MewUI GitHub](https://github.com/aprillz/MewUI)
- [MewUI Documentation](https://github.com/aprillz/MewUI/tree/main/docs)
- [MewUI Samples](https://github.com/aprillz/MewUI/tree/main/samples)
- VS Code UI 参考：https://github.com/microsoft/vscode

---

## 下一步行动

1. ✅ 创建项目结构
2. ✅ 定义核心接口
3. ✅ 实现 ShellContext
4. ✅ 搭建 UI 骨架
5. → **周一开始编码**
