# ADR 0015: Scope Workbench Content to Activity Workspaces

## Status

Accepted

## Context

Mewoo borrows VSCode's ActivityBar, Sidebar, MainArea, Panel, and StatusBar structure, but it is not primarily an editor. In VSCode, ActivityBar selection changes the Sidebar while editor tabs remain global. In Mewoo, Activities are plugin-contributed product modules, so keeping MainArea, Panel, and StatusBar globally shared can mix one Activity's navigation with another Activity's content.

## Decision

Mewoo will treat each Activity as an `Activity Workspace`. The ActivityBar remains global, but the active Activity Workspace owns its Sidebar, MainArea tab stack, Panel state/content, and Activity-scoped StatusBar items.

`OwnerPluginId` and `ActivityScopeId` are separate identities. `OwnerPluginId` controls lifecycle, unload, permissions, diagnostics, and contribution ownership. `ActivityScopeId` controls where UI contributions appear and which Activity Workspace state they belong to.

Opening a MainArea view should switch to that view's owning Activity Workspace. Activity scope may be inferred for simple single-Activity plugins, but multi-Activity plugins must declare or provide enough structure to infer Activity scope. Global contributions must opt into global scope explicitly.

## Consequences

- Switching Activity restores that Activity's own MainArea tabs and active tab instead of sharing one global tab stack.
- Panel visibility, height, active tab, and Activity-scoped panel tabs become Activity Workspace state. Global content such as Logs can be opened from any Activity Workspace without making Panel visibility global.
- StatusBar contributions are Activity-scoped by default, with explicit global scope reserved for shell-level or cross-Activity state.
- The workbench intentionally diverges from VSCode's global editor-tab model because Mewoo Activities represent module workspaces rather than editor sidebars.
