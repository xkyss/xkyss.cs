# ADR 0016: Add an ActivityBar System Section

Date: 2026-06-03

## Status

Accepted

## Context

Mewoo now treats each Activity selection as an `Activity Workspace`: Sidebar, MainArea, Panel, and Activity-scoped StatusBar content switch together. Some Activities, however, are shell/system management surfaces rather than ordinary product modules.

`Plugin Manager` is a built-in management surface for local plugins. `Settings` will become a shell-level entry point. Keeping these entries in the same ActivityBar column is useful, but placing them beside ordinary product Activities makes the ActivityBar feel like a flat list of unrelated modules.

VSCode-like shells commonly reserve the bottom of the activity bar for management and account-style entries. Mewoo needs the same affordance without weakening the Activity Workspace model.

## Decision

ActivityBar will support two sections:

- `Primary`: the default top section for ordinary product Activities.
- `System`: a bottom section for built-in system Activities.

ActivityBar section is a UI placement and authorization property. It does not create a special Activity kind. Selecting a system-section Activity still switches the full Activity Workspace.

The `System` section is reserved for compiled built-in plugins. Runtime or locally installed plugins cannot contribute system-section Activities. If a runtime/local plugin declares a system-section Activity, contribution registration fails and the existing plugin failure/diagnostics path surfaces the problem.

V6 will not support user sorting or hiding of system-section Activities. Ordering is fixed by contribution order. The initial system-section order is:

1. Plugin Manager
2. Settings

Plugin Manager remains a built-in plugin contribution rather than a shell-hardcoded Activity. Settings will also be implemented as a built-in/system Activity skeleton.

## Consequences

- The Activity Workspace model remains consistent for both ordinary and system Activities.
- Plugin Manager can move to a bottom management area without becoming a shell exception.
- Third-party/runtime plugins cannot compete for privileged ActivityBar placement.
- Settings can validate the system-section model without requiring a complete settings schema in V6.
- A future marketplace or permission system can add explicit approval for privileged placement, but V6 does not need that complexity.

## Deferred

- User-configurable ordering or hiding of system Activities.
- Manifest permissions for system-section placement.
- Marketplace review or user authorization for privileged ActivityBar placement.
- Plugin-contributed Settings pages.
