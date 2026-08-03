# Changelog

All notable changes to Mewoo are recorded here.

## 0.8.0 - 2026-06-05

### Features

- Unified runtime diagnostics into the Plugin Manager System Activity Workspace.
- Split Plugin Manager registration, session state, controller actions, formatting, and view rendering into dedicated classes.
- Reworked Plugin Manager Sidebar navigation around management categories and a collapsed Advanced diagnostics group.
- Removed the standalone Runtime Diagnostics ActivityBar entry.
- Added Plugin Manager advanced views for runtime status, discovery issues, and operation logs.
- Kept Plugin Details, install preview, and update preview as dedicated MainArea views.

### Tests

- Added focused Plugin Manager tests for session/view structure, Sidebar category navigation, Runtime Diagnostics retirement, advanced diagnostic views, and the V8 unified workspace road test.
- Verified V8 with focused Plugin Manager, RuntimePlugin, ActivityWorkspace, ActivityBarSystem, and Sidebar test filters plus `dotnet build Mewoo.slnx --no-restore --verbosity minimal`.

## 0.7.0 - 2026-06-04

### Features

- Added the optional `Mewoo.Controls` plugin UI helper library.
- Added reusable Sidebar layout, header, search row, More menu, and multi-level collapsible `SidebarNavigation` helpers.
- Exposed minimal read-only Workbench view state through `IWorkbenchService` for plugin UI selected-state behavior.
- Changed Workbench Sidebar hosting so `ViewContainer.Title` and `SidebarView.Title` remain metadata instead of being rendered as duplicate visible chrome.
- Migrated built-in Sidebars to the new optional Sidebar controls while leaving MainArea layouts plugin-owned.

### Tests

- Added focused tests for Workbench view state, Sidebar header/search controls, SidebarNavigation behavior, plugin-owned Sidebar rendering, and the V7 Sidebar controls road test.
- Verified V7 with `dotnet build Mewoo.slnx --no-restore --verbosity minimal` and `dotnet test Mewoo.slnx --no-restore --verbosity minimal`.

## 0.6.0 - 2026-06-03

### Features

- Added Primary and System ActivityBar sections so built-in management Activities can stay pinned at the bottom.
- Restricted System Activity registration to built-in plugins, with clear failure diagnostics for runtime plugins.
- Moved Plugin Manager into the bottom System ActivityBar section.
- Added the built-in Settings Activity skeleton with its own Activity Workspace.

### Tests

- Added focused tests for ActivityBar section metadata, rendering model ordering, built-in system Activities, runtime-plugin rejection, and the V6 ActivityBar system-section road test.
- Verified V6 with `dotnet build Mewoo.slnx --no-restore --verbosity minimal` and `dotnet test Mewoo.slnx --no-restore --verbosity minimal`.

## 0.5.0 - 2026-05-29

### Features

- Added Activity Workspace scoping so Activity switches now isolate Sidebar, MainArea tabs, Panel state, and Activity-scoped StatusBar items.
- Added per-Activity MainArea tab stacks and made MainView commands switch to their owning Activity Workspace.
- Added per-Activity Panel visibility, height, active panel tab, and Logs panel opening behavior.
- Added Activity-scoped StatusBar rendering with explicit global StatusBar items.
- Completed the V5 Activity Workspace road test coverage.

### Tests

- Added focused tests for Activity Workspace scope inference, per-Activity MainArea tabs, Panel scope, StatusBar scope, and V5 road test behavior.
- Verified the full solution with `dotnet build Mewoo.slnx --no-restore --verbosity minimal` and `dotnet test Mewoo.slnx --no-restore --verbosity minimal`.

## Historical Baselines

- `0.4.0` / V4: Local Plugin Manager product surface, local package install/update flows, broken install cleanup, author workflow, and V4 baseline readiness.
- `0.3.0` / V3: Local plugin package format, installer, uninstall/update operations, catalog metadata, trust declarations, and isolation exploration.
- `0.2.0` / V2: Runtime plugin manifest discovery, runtime assembly loading, lifecycle operations, diagnostics, compatibility handling, and authoring guide.
- `0.1.0` / V1: Runnable shell skeleton, compiled plugin contribution model, workbench layout, command execution, theme switching, persistence, Quick Launcher, logs, and plugin failure feedback.
