# Mewoo Context

Mewoo is a personal desktop UI template built on top of MewUI. It provides a VSCode-like host shell for small personal applications that can be plugged into the shell.

Mewoo is not primarily a code editor. It borrows VSCode's information architecture and interaction model: an activity bar, sidebar, editor/document area, bottom panel, status bar, command registry, theme tokens, and contribution points. The hosted apps provide their own domain UI and behavior.

## First Real Apps

- Quick launcher: a navigation-style app for opening websites, local software, scripts, and other shortcuts.
- Trading K-line analyzer: a candle-by-candle analysis tool for trading workflows.

## First Version Scope

The first version focuses on the stable shell shape:

- Fixed VSCode-like regions.
- Collapsible sidebar and bottom panel.
- Tabbed document/editor area.
- Command registry.
- Theme tokens.
- Status bar contribution points.
- Early plugin-system design, even if full dynamic plugin loading comes later.

## Plugin Model

The first version uses compiled plugins: C# modules referenced by the host and registered at startup. Runtime assembly scanning, plugin unloading, version isolation, and hot reload are out of scope for the initial version.

Multiple plugins may run at the same time. A plugin may contribute one or more Activity Bar items.

Initial contribution points:

- Activity Bar items.
- Sidebar views.
- MainArea tabs/views.
- Bottom panel views.
- Commands.
- Status bar items.
- Optional theme token overrides.

The central tabbed content region is called `MainArea`, not `EditorArea`, because Mewoo is not primarily an editor.

The first tracer bullet is a Quick Launcher plugin.

## Workbench Identity Model

The active Activity Bar selection is an `ActivityId`, not a `PluginId`.

Activity Bar contributions use this shape:

```text
ActivityBarItem -> ViewContainer -> SidebarView[]
```

Plugins open MainArea content through a shell-owned workbench service instead of directly mutating tabs. The shell owns tab activation, deduplication, close behavior, and future restore behavior.

Command IDs are globally namespaced strings such as `quickLauncher.open` and `workbench.toggleSidebar`.

Mewoo needs a full plugin lifecycle. The exact lifecycle states and hooks are still being defined.

## Plugin Runtime Roadmap

Mewoo uses a phased plugin strategy:

- V1: compiled plugins with runtime lifecycle management.
- V2: manifest-based runtime assembly loading.
- V3: stronger isolation and marketplace-like install/update behavior.

V1 should design lifecycle and contribution ownership so V2 can be added later, but V1 does not implement runtime assembly scanning, plugin manifests, dependency isolation, hot reload, or plugin marketplace behavior.

## V1 Plugin Lifecycle

V1 lifecycle states:

```text
Created -> Registered -> Activated -> Deactivated -> Unloaded -> Disposed
Any state -> Failed
```

Registration and runtime lifecycle are separate. A plugin declares contributions during registration, and performs runtime work during activation.

Lifecycle rules:

- Deactivated plugin contributions are hidden by default.
- Unloading a plugin closes MainArea tabs owned by that plugin.
- Plugin failures must not crash the shell.
- `Register` failure makes the plugin invisible.
- `ActivateAsync` failure hides the plugin's contributions and records the error.
- Cleanup failures are recorded, but shell cleanup continues.
- Background tasks must be shell-managed and cancellation-aware.
- V1 does not implement plugin dependency resolution.
- V1 defines plugin-scoped storage through `IPluginStorage`.

## Project Structure

Mewoo is split into:

- `Mewoo.Abstractions`: plugin interfaces, contribution descriptors, command descriptors, theme tokens, storage contracts, and shared shell-facing contracts.
- `Mewoo.Core`: plugin management, command registry, lifecycle orchestration, contribution ownership, state, and lightweight services.
- `Mewoo.Workbench`: MewUI-based ActivityBar, Sidebar, MainArea, Panel, and StatusBar shell implementation.
- `Mewoo.Plugins.QuickLauncher`: first tracer bullet plugin.
- `Mewoo.App`: desktop entry point that composes the workbench and compiled plugins.

Plugins should primarily reference `Mewoo.Abstractions`.

V1 UI is code-first. Plugin views are created through lazy factories rather than eagerly-created controls.

V1 uses a lightweight service registry unless a stronger DI framework becomes necessary.

The Quick Launcher V1 model includes groups and launcher items with URL, file, executable, or script targets.

## Plugin Contribution API

Plugins register contributions through a fluent builder API. Internally, the registry stores descriptor objects for validation, ownership, lifecycle, and rendering.

Contribution IDs are globally namespaced strings such as `quickLauncher.open`. Core validates duplicate IDs and ID format.

The registry automatically assigns the current plugin as contribution owner.

View factories are lazy and return `IMewooView`; registration must not create UI controls.

Commands are asynchronous and return `ValueTask`.

StatusBar contributions can be updated at runtime through shell-provided handles.

Plugins may override theme tokens but may not register full themes in V1.

## Command, State, and Theme Systems

Commands are globally namespaced, asynchronous, and owned by plugins or the shell. Duplicate command IDs are errors. Default keybinding conflicts are errors in V1. Keybinding metadata may exist before a full keybinding resolver.

Shell state uses mutable models with change events. Shell state includes active Activity, sidebar collapsed/width, panel visibility/height, active MainArea view, open MainArea views, theme ID, and always-on-top state.

Shell state and plugin state are separate. Shell state is saved by Mewoo; plugin state is saved through `IPluginStorage`.

State persistence uses JSON behind storage abstractions such as `IStateStorage` and `IPluginStorage`.

Themes use strongly typed base tokens plus extension key-values. V1 includes Dark and Light themes. High Contrast is reserved for later. Plugins may override tokens but may not register full themes in V1.

## V1 Workbench Shell Layout

Mewoo V1 uses a borderless main window with a custom title bar.

The full V1 visual layout is documented in `docs/design/v1-ui-visual-design.md`.

Top-level layout:

```text
Root
├─ CustomTitleBar
├─ Workbench
│  ├─ ActivityBar
│  ├─ Sidebar
│  ├─ MainArea
│  ├─ Panel
│  └─ StatusBar
```

The custom title bar contains app icon/name/version, function menus, current MainArea or active workbench title, theme and always-on-top actions, and minimize/maximize/close controls.

In V1, title-bar menus and right-side title-bar actions are shell-owned. Plugin status belongs in the StatusBar, not in the TitleBar.

The center title is provided by a workbench title service with this priority: active MainArea tab title, then active Activity title, then app name.

Always-on-top state is persisted as a shell preference.

The custom borderless window must support dragging, double-click maximize/restore, minimize/maximize/close, resizable edges, correct maximize behavior, and window-state-aware controls.

TitleBar height is fixed at `34px`.

ActivityBar items are icon-only with hover tooltips. Selecting an ActivityBar item shows its corresponding Sidebar `ViewContainer`.

MainArea supports multiple tabs but not split editor groups in V1.

Sidebar and Panel are resizable. Panel exists in V1 but is hidden by default.

## MewUI Baseline

The intended baseline is MewUI v0.15.2 because it is the latest published release. The local MewUI source may be newer than v0.15.2, so implementation work should verify API details against the chosen source before coding against MewUI-specific types.

MewUI details are isolated in `Mewoo.Workbench`. `Mewoo.Abstractions` does not reference MewUI. Plugins may use MewUI to build their own views, but expose them through `IMewooView.NativeView`.

Workbench validates native views, maps theme tokens to MewUI styling, handles unsupported plugin views with error UI, and encapsulates borderless-window behavior.

## Quick Launcher Tracer Bullet

The first plugin is `Mewoo.Plugins.QuickLauncher`.

Initial UI:

- ActivityBar launcher icon.
- Sidebar search, groups, and recent list.
- MainArea launcher tab with search, list of launcher items, and item actions.
- StatusBar item count, selected item, or last run result.

Quick Launcher V1 uses JSON configuration and supports URL, file, executable, and script targets.

Initial commands:

- `quickLauncher.open`
- `quickLauncher.focusSearch`
- `quickLauncher.runSelected`
- `quickLauncher.reload`
- `quickLauncher.openConfig`

V1 starts with list UI. Grid view, icon downloading, drag-and-drop sorting, cloud sync, advanced fuzzy search, and complex script security are out of scope.
