# MewPad

基于 MewUI 和 .NET 10 的桌面 Shell 框架，提供 VS Code 风格布局、主题与语言切换、设置系统、可扩展活动栏/面板/内容区。

## 当前状态

- P0-P4 已完成
- P5 已完成发布链路准备（含 NativeAOT x64 验证）
- 测试现状：45/45 通过

## 目录结构

```text
src/
	MewPad.Core/
	MewPad.Hosting/
tests/
	MewPad.Tests/
doc/
```

## 本地开发

```powershell
dotnet restore
dotnet build
dotnet test tests\MewPad.Tests\MewPad.Tests.csproj
dotnet run --project .\src\MewPad.Hosting\
```

## 发布说明（P5）

### 1) 常规 Release 构建

```powershell
pwsh .\build-release.ps1 -Version 0.1.0-preview
```

输出目录：`.build\release\framework`

### 2) NativeAOT x64 发布

已参考 Resty 的方式实现：先初始化 VS Build Tools 环境，再执行 AOT publish。

```cmd
publish-aot.cmd
```

或透传 dotnet publish 参数：

```cmd
publish-aot.cmd -v minimal
```

核心实现文件：

- `publish-aot.cmd`
- `src/MewPad.Hosting/Properties/PublishProfiles/win-x64-aot.pubxml`

默认依赖路径：

- `D:\Scoop\apps\vsbuildtools2022\current\vs\Common7\Tools\VsDevCmd.bat`

### 3) 一键完整发布（含 AOT）

```powershell
pwsh .\build-release.ps1 -Version 0.1.0-preview
```

输出目录：

- `.build\release\framework`
- `.build\release\aot-win-x64`
- `.build\release\MewPad-<version>-win-x64-aot.zip`
- `.build\release\CHECKSUMS.txt`

如仅做常规发布，跳过 AOT：

```powershell
pwsh .\build-release.ps1 -Version 0.1.0-preview -SkipAot
```

## 已验证结果

- NativeAOT 发布命令执行成功
- 产物位于：`.build\MewPad.Hosting\bin\Release\net10.0\win-x64\publish`
- 主程序：`MewPad.Hosting.exe`

## 文档索引

- `doc/01-overview.md`
- `doc/02-layout.md`
- `doc/03-components.md`
- `doc/04-extensibility.md`
- `doc/05-global-features.md`
- `doc/06-development-plan.md`
- `doc/07-quick-start.md`
- `doc/07-tasks.md`