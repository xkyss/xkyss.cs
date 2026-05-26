# ADR 0006: Split Mewoo into Abstractions, Core, Workbench, Plugins, and App

## Status

Accepted

## Context

Mewoo needs to host multiple compiled plugins while keeping plugin authors away from shell internals. The shell is implemented with MewUI, but plugin contracts should remain as small and stable as possible.

The first tracer bullet is the Quick Launcher plugin. It should validate the workbench shell, contribution model, command registry, status bar contribution, plugin lifecycle, and MainArea behavior without forcing the whole architecture to depend on one app.

## Decision

Mewoo will use this project structure:

```text
Mewoo.Abstractions
  Plugin interfaces, contribution descriptors, command descriptors,
  theme tokens, storage contracts, and shared shell-facing contracts.

Mewoo.Core
  Plugin management, command registry, lifecycle orchestration,
  contribution ownership, state, and lightweight services.

Mewoo.Workbench
  MewUI-based VSCode-like shell implementation:
  ActivityBar, Sidebar, MainArea, Panel, and StatusBar.

Mewoo.Plugins.QuickLauncher
  First tracer bullet plugin.

Mewoo.App
  Desktop entry point that composes the workbench and compiled plugins.
```

Plugins should primarily reference `Mewoo.Abstractions`.

V1 UI is code-first. Plugin views are created through lazy factories rather than eagerly-created controls. If a MewUI base control type is needed, the contract may use that type; otherwise, contribution descriptions should avoid leaking workbench internals.

V1 will use a lightweight service registry instead of introducing a heavy dependency injection framework, unless MewUI or the surrounding codebase makes a DI framework necessary.

The Quick Launcher V1 data model is intentionally small:

- Group.
- Launcher item.
- Kind: URL, file, executable, or script.
- Title.
- Target.
- Arguments.
- Optional icon.
- Optional tags.

## Consequences

The plugin contract can stay stable while the MewUI-based workbench evolves.

Core lifecycle and contribution logic can be tested without rendering the full UI.

The first coding phase can start with `Mewoo.Abstractions` and `Mewoo.Core`, which do not require all MewUI API details to be finalized.

Workbench implementation still requires checking the actual MewUI v0.15.2 API before binding to concrete UI types.

