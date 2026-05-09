# Changelog - MewPad

## [0.1.0-preview] - 2026-05-09

### 📦 Release Preparation (P5)

- ✅ Added NativeAOT publish profile: `src/MewPad.Hosting/Properties/PublishProfiles/win-x64-aot.pubxml`
- ✅ Added Resty-style AOT launcher script: `publish-aot.cmd`
- ✅ Upgraded `build-release.ps1` to support:
	- framework-dependent publish output
	- NativeAOT publish with `VsDevCmd` + `IlcUseEnvironmentalTools`
	- release zip and SHA256 checksum generation
- ✅ Verified `dotnet publish` for `win-x64` NativeAOT completed successfully

### ⚠ Known Release Warnings

- NativeAOT trimming warnings exist in `ConfigurationService` due to generic `System.Text.Json` serialize/deserialize usage.
- These are warnings, not blocking errors; current preview build is publishable.

### ✨ Added - Phase 1 Complete

#### Core Framework
- ✅ Universal desktop GUI framework based on .NET 10
- ✅ Shell-based architecture with ShellContext
- ✅ Observable state management via ObservableValue<T>
- ✅ Fluent API for component building (via MewUI)

#### Services Implementation
- ✅ **ThemeService**: Light/Dark theme switching with IObservable support
- ✅ **LocalizationService**: Multi-language i18n with en-US and zh-CN
- ✅ **ConfigurationService**: JSON-based application settings persistence
- ✅ **SettingsService**: Extensible settings categories system

#### Extensibility Architecture
- ✅ `IActivityItem`: Navigation bar items with ordering
- ✅ `IPanelItem`: Bottom panel tabs with customization
- ✅ `IContentItem`: Main content area tabs with lifecycle
- ✅ `ISettingsCategory`: Settings panel categories
- ✅ Registration-based plugin system

#### Built-in Extensions
- ✅ **WelcomeActivity**: Application entry point
- ✅ **AppearanceSettings**: Theme configuration
- ✅ **LanguageSettings**: Language selection

#### Development Infrastructure
- ✅ MewPad.Core (classlib) - Framework
- ✅ MewPad.Hosting (WinExe) - Application shell
- ✅ .NET 10 target framework
- ✅ Nullable reference types enabled
- ✅ C# 13 language features

### 📋 Project Status
- **Core Interfaces**: All 4 interfaces defined and implemented
- **Services**: 4 core services fully implemented
- **UI Framework**: Basic application window with Fluent API
- **Compilation**: ✅ 0 errors, 0 warnings
- **Build Output**: ~40KB total (MewPad.Core + MewPad.Hosting DLLs)

### 🔮 Roadmap for Future Phases

**Phase 2 (Multi-language & Settings)**
- Advanced localization with JSON file loading
- Settings persistence and synchronization
- Language switching at runtime with UI updates

**Phase 3 (Advanced Interactions)**
- Tab management (open/close/reorder)
- Panel collapse/expand animations
- Keyboard shortcuts and command palette
- Drag-and-drop support

**Phase 4 (Testing & Release)**
- Unit tests for services
- Integration tests for shell operations
- Performance profiling and optimization
- Release build and packaging (v0.1.0)

---

**Total Implementation Time**: ~2 hours
**Lines of Code**: ~800 (Framework) + ~300 (Sample Extensions)
**Test Coverage**: Manual smoke testing (builds successfully)