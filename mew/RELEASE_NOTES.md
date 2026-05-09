# MewPad v0.1.0-preview Release Notes

## 🎯 Overview

**MewPad** is a universal desktop GUI framework based on **.NET 10** and **MewUI**, designed to provide a modern, extensible foundation for building desktop applications.

This is the **first preview release**, showcasing the core framework architecture and extensibility model.

## ✨ Key Features

### 🏗️ Modern Architecture
- **Shell-based design** inspired by VS Code
- **Observable state management** for reactive UI updates
- **Registration-based plugin system** for extensibility
- **Service-oriented architecture** with dependency injection

### 🌍 Multi-language & Theming
- Built-in **light/dark theme support** with runtime switching
- **i18n support** with en-US and zh-CN included
- Dynamic language switching without application restart

### 🔌 Extensibility Framework
- Define custom **activities** (sidebar navigation)
- Add **panels** for workspace tools
- Create **content tabs** for main workspace
- Implement **settings categories** for configuration

### 🚀 Developer Experience
- **Fluent API** for component building
- **XML documentation** on all public APIs
- **Nullable reference types** enabled for safety
- **C# 13** language features support

### 📦 Release Pipeline (P5)
- Added one-click NativeAOT publish script (`publish-aot.cmd`)
- Added AOT publish profile (`win-x64-aot.pubxml`)
- Updated `build-release.ps1` for framework + AOT packaging
- Added checksum generation for release archive

## 📦 What's Included

```
MewPad/
├── src/
│   ├── MewPad.Core/          # Framework (DLL)
│   │   ├── Interfaces/       # Extension contracts
│   │   ├── Services/         # Core services + implementations
│   │   └── Shell/            # ShellContext (state hub)
│   └── MewPad.Hosting/        # Application (EXE)
│       └── Extensions/       # Sample implementations
├── doc/                       # Design documentation
├── appsettings.json          # Application config
└── README.md                 # Getting started
```

## 🚀 Quick Start

### Build
```bash
cd MewPad
dotnet build -c Debug
```

### Run
```bash
# From solution directory
dotnet run --project src/MewPad.Hosting/MewPad.Hosting.csproj
```

### Create Custom Extension

1. **Implement IActivityItem** for navigation:
```csharp
public class MyActivity : IActivityItem
{
    public string Id => "myactivity";
    public object Icon => "🔍";
    public string Title => "My Tool";
    
    public FrameworkElement CreateContent()
    {
        return new Label().Text("Hello World");
    }
}
```

2. **Register in ShellContext**:
```csharp
var shell = new ShellContext();
shell.RegisterActivity(new MyActivity());
```

3. **Activity appears in sidebar automatically!**

## ⚙️ System Requirements

- **.NET 10** runtime or SDK
- **Windows 10+**, **Linux**, or **macOS**
- **VS Code** or **Visual Studio 2022+** (for development)

## 🐛 Known Limitations (Preview)

- UI components use simplified placeholder layouts
- No drag-and-drop support yet
- Limited keyboard shortcut system
- Command palette not implemented
- NativeAOT build emits trimming/AOT analysis warnings in `ConfigurationService`

## 📚 Documentation

- **[README.md](README.md)** - Project overview and quick start
- **[doc/01-overview.md](doc/01-overview.md)** - Design principles
- **[doc/02-layout.md](doc/02-layout.md)** - UI layout specification
- **[doc/03-components.md](doc/03-components.md)** - Component architecture
- **[doc/04-extensibility.md](doc/04-extensibility.md)** - Extension guide
- **[doc/05-global-features.md](doc/05-global-features.md)** - Features spec
- **[doc/06-development-plan.md](doc/06-development-plan.md)** - Roadmap

## 🛠️ Development

```bash
# Install dependencies
dotnet restore

# Build solution
dotnet build

# Build release
dotnet build -c Release

# Publish standalone
dotnet publish -c Release -r win-x64 --self-contained

# Run tests (future)
dotnet test
```

## 📋 Verification Checklist

- [x] Solution compiles without errors
- [x] MewPad.Core loads successfully
- [x] MewPad.Hosting application window opens
- [x] ThemeService toggles themes
- [x] LocalizationService supports en-US and zh-CN
- [x] ShellContext registers extensions
- [x] Built-in activities display correctly
- [x] 45 automated tests pass
- [x] NativeAOT x64 publish succeeds

## 🤝 Contributing

This project is designed to be extensible from day one. To contribute:

1. Fork or clone the repository
2. Create a feature branch
3. Implement your feature following the extension patterns
4. Submit a pull request with documentation

## 📄 License

MIT License - See LICENSE file for details

## 🔗 Links

- **GitHub**: [xkyss/MewPad](https://github.com/xkyss/MewPad)
- **MewUI**: [Aprillz.MewUI NuGet](https://www.nuget.org/packages/Aprillz.MewUI)
- **.NET**: [Microsoft .NET](https://dotnet.microsoft.com)

---

**Version**: 0.1.0-preview  
**Release Date**: 2026-05-09  
**Status**: ✅ Preview Ready for Feedback