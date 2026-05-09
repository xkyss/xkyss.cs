# 08 - 插件开发指南

## 目标

本文档定义 MewPad 的插件开发方式，目标是让插件可以单独项目开发、独立构建，并在宿主中加载与运行。

当前建议采用两阶段策略：

1. 第一阶段：进程内插件（最小可用）
2. 第二阶段：动态加载器（目录扫描 + 反射加载）

当前状态（P6 MVP）：

1. 已支持从 `plugins` 目录动态加载 DLL
2. 已支持两种入口：`IPlugin` 实现类、`public static void Register(ShellContext)`

---

## 当前可用扩展点

MewPad.Core 已提供以下扩展接口，可直接用于插件开发：

- IActivityItem
- IPanelItem
- IContentItem
- ISettingsCategory
- StatusBarItem

当前项目内已经通过 shell.RegisterActivity / RegisterPanel / OpenContent / RegisterStatusBarItem 完成接入。

---

## 插件项目推荐结构

```text
QuickLaunch.Plugin/
├── QuickLaunch.Plugin.csproj
├── QuickLaunchPluginEntrypoint.cs
└── README.md
```

推荐规范：

1. 一个插件一个独立项目
2. 插件项目仅依赖 MewPad.Core（必要时可依赖 MewUI）
3. 插件入口统一为 Register(ShellContext shell)
4. 所有插件 ID 使用唯一前缀（例如 quicklaunch.*）

---

## 最小插件入口约定（V1）

在动态加载器完成前，先采用约定式入口：

```csharp
public static class PluginEntrypoint
{
    public static void Register(ShellContext shell)
    {
        // 注册 Activity / Panel / StatusBar / Settings
    }
}
```

说明：

1. 入口方法必须是 public static Register(ShellContext)
2. 插件可只注册一类扩展，也可同时注册多类
3. 注册逻辑应保证幂等，避免重复注册导致重复 UI

---

## 生命周期建议

1. 宿主启动
2. 发现插件程序集
3. 加载程序集
4. 调用 Register
5. 注册到 ShellContext
6. 插件 UI 随 Shell 一起工作

后续会补充 Unload/Dispose 约定，用于热重载或停用插件。

---

## 本地验证动态加载（MVP）

1. 先构建宿主与示例插件

```powershell
dotnet build src\MewPad.Hosting\MewPad.Hosting.csproj
dotnet build samples\QuickLaunch.Plugin\QuickLaunch.Plugin.csproj
```

2. 将插件 DLL 复制到宿主输出目录的 `plugins` 子目录

```powershell
$hostOut = ".build\MewPad.Hosting\bin\Debug\net10.0"
$pluginOut = ".build\QuickLaunch.Plugin\bin\Debug\net10.0\QuickLaunch.Plugin.dll"
New-Item -ItemType Directory -Force -Path (Join-Path $hostOut "plugins") | Out-Null
Copy-Item $pluginOut (Join-Path $hostOut "plugins") -Force
```

3. 启动宿主

```powershell
dotnet run --project .\src\MewPad.Hosting\
```

4. 验收结果

1. ActivityBar 出现 ⚡ 快捷启动
2. StatusBar 右侧出现 `⚡ QuickLaunch`

---

## 版本兼容建议

插件端建议声明以下信息（后续可落到 plugin.json）：

- PluginId
- Version
- MinHostVersion
- TargetFramework

宿主侧在动态加载时做最低版本校验，避免接口不兼容。

---

## 调试建议

1. 在插件项目中使用独立单元测试验证逻辑
2. 在宿主中启用插件日志（加载成功/失败、异常堆栈）
3. 为每个插件注册唯一状态栏项，便于确认插件是否生效

---

## 示例插件

示例插件位于：

- samples/QuickLaunch.Plugin/QuickLaunch.Plugin.csproj
- samples/QuickLaunch.Plugin/QuickLaunchPluginEntrypoint.cs
- samples/QuickLaunch.Plugin/README.md

示例插件提供一个 快捷启动 Activity，并演示：

1. 打开设置
2. 切换主题
3. 打开插件自定义内容页
