# QuickLaunch.Plugin

QuickLaunch 是 MewPad 的示例插件，演示最小插件接入方式。

## 功能

1. 注册一个 ActivityBar 入口（⚡）
2. 注册一个右侧状态栏项（⚡ QuickLaunch）
3. 在 Activity 内容中提供常用动作按钮
4. 打开一个插件自定义内容页（quicklaunch.guide）

## 构建

```powershell
cd samples\QuickLaunch.Plugin
dotnet build
```

## 在宿主中接入（当前方式）

在 MewPad.Hosting 的 Program.cs 中手动调用：

```csharp
QuickLaunch.Plugin.QuickLaunchPluginEntrypoint.Register(shell);
```

## 动态加载（推荐）

1. 使用 `plugin.json` 描述插件元数据
2. 宿主会扫描 `plugins/<插件目录>/plugin.json`
3. 依据 `entryAssembly` 加载插件 DLL

本示例包含：

- `plugin.json`
- `QuickLaunch.Plugin.dll`

## 后续动态加载方式（规划中）

后续 PluginLoader 完成后，插件将支持目录扫描与反射加载，无需改宿主 Program.cs。
