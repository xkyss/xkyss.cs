# Issue 057: Workbench Read-Only View State

Status: Planned

## What to build

Expose a minimal read-only Workbench view state through the plugin-facing Workbench service so controls and plugins can react to the active Activity and MainArea view without mutating Workbench internals.

## Acceptance criteria

- [ ] `IWorkbenchService` exposes a read-only current state with `ActiveActivityId` and `ActiveMainViewId`.
- [ ] `IWorkbenchService` exposes a state-changed notification for changes relevant to plugin UI helpers.
- [ ] Plugins cannot access or mutate `WorkbenchState` directly.
- [ ] MainArea tabs, Panel dictionaries, Sidebar width, and persistence internals remain hidden.
- [ ] Tests cover state snapshot values and state-changed notifications when Activity/MainArea selection changes.

## Notes

- This supports automatic selected-state behavior for `SidebarNavigation` main-view items.
- MainArea opening must still go through `IWorkbenchService.OpenMainViewAsync`.

## Blocked by

- Issue 056

