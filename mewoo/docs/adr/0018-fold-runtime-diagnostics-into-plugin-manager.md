# ADR 0018: Fold Runtime Diagnostics into Plugin Manager

## Status

Accepted

## Context

Plugin Manager and Runtime Diagnostics both operate on local plugins, but earlier milestones exposed them as separate Activities. V4 made Plugin Manager the user-facing surface for install, update, enable, disable, uninstall, and broken install cleanup. Runtime Diagnostics remained the developer-facing surface for load state, manifest paths, assembly paths, runtime issues, and logs.

V5 then scoped Sidebar, MainArea, Panel, and StatusBar state to Activity Workspaces. V6 moved Plugin Manager to the bottom System ActivityBar section while Runtime Diagnostics remained a primary Activity. V7 made Sidebar and MainArea interiors plugin-owned and encouraged plugin-provided Sidebar navigation instead of Workbench-rendered headers.

Keeping Runtime Diagnostics as a separate Activity now makes plugin management feel split across two workspaces even though both surfaces describe the same local plugin domain. A simple MainArea toggle inside Plugin Manager would also create a second navigation system inside the view, conflicting with the V7 direction that Sidebar owns this kind of navigation.

## Decision

Mewoo V8 will fold Runtime Diagnostics into the Plugin Manager Activity Workspace.

The `Plugins` System Activity will own both user-facing management views and advanced diagnostic views. Plugin Manager Sidebar navigation will expose management categories and an advanced diagnostics group. MainArea content remains ordinary Workbench-hosted MainView tabs; Plugin Manager will not add an internal category TabControl or mode toggle inside its MainArea views.

Management views answer user questions such as what is installed, whether a plugin is enabled, what package can be installed or updated, and what user-facing operation is available. Advanced diagnostic views answer developer questions such as what manifest or assembly path was discovered, whether runtime activation failed, what low-level issue occurred, and which diagnostic action is useful.

Runtime Diagnostics no longer contributes its own ActivityBar item. Its developer-facing fields and actions remain available through Plugin Manager advanced views.

The independent `RuntimeDiagnosticsPlugin` contribution will be retired. Runtime diagnostic UI capabilities will move into Plugin Manager advanced MainViews, while shared runtime manager, catalog display, logging, and package operation services remain in core infrastructure.

## Consequences

- Plugin-related management and diagnostics live in one Activity Workspace, reducing cross-Activity navigation for the same domain.
- Plugin Manager remains a built-in System Activity rather than becoming a Workbench special case.
- The ActivityBar is quieter: runtime diagnostics no longer competes with ordinary primary Activities.
- Sidebar navigation carries the management-versus-advanced split, preserving the V7 plugin-owned Sidebar model.
- MainArea tabs remain shell-managed MainViews instead of an internal Plugin Manager tab system.
- Runtime Diagnostics implementation can be reused or migrated behind Plugin Manager advanced views, but its user-facing ActivityBar contribution is removed.
- The `runtimeDiagnostics.*` Activity, ViewContainer, MainView, and command IDs become obsolete in V8. New diagnostic views use `pluginManager.*` identities.
