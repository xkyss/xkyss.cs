# 10 - 插件模板与版本化发布

## 插件项目模板（P6.4）

建议以如下模板创建新插件：

```text
MyPlugin/
├── MyPlugin.csproj
├── plugin.json
├── MyPlugin.cs
└── README.md
```

## csproj 模板

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\MewPad.Core\MewPad.Core.csproj" />
  </ItemGroup>
</Project>
```

## 入口代码模板

```csharp
using MewPad.Core.Plugins;
using MewPad.Core.Shell;

public sealed class MyPlugin : IPlugin
{
    public string Id => "my.plugin";

    public void Register(ShellContext shell)
    {
        // 注册 Activity / Panel / StatusBar / Settings
    }
}
```

## plugin.json 模板

```json
{
  "id": "my.plugin",
  "name": "My Plugin",
  "version": "0.1.0",
  "minHostVersion": "0.1.0",
  "entryAssembly": "MyPlugin.dll"
}
```

## 版本化发布建议

1. 插件版本采用语义化版本：`MAJOR.MINOR.PATCH`
2. 每次发布更新：`plugin.json.version`
3. 发生 API 破坏变更时，提升 MAJOR
4. 发布包建议包含：
   1. plugin.json
   2. 主 DLL
   3. README
   4. CHANGELOG（可选）

## 宿主兼容策略

1. 宿主按 `minHostVersion` 做门禁
2. 不兼容插件不加载，写入错误日志
3. 禁用插件由配置项 `plugins.disabledIds` 控制

## 发布验收清单

1. 插件可单独编译
2. plugin.json 完整且字段合法
3. 放入 `plugins/<plugin>/` 后宿主可加载
4. 不兼容版本场景有清晰错误信息
