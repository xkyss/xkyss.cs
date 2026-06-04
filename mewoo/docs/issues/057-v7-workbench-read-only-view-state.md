# Issue 057: Workbench Read-Only View State

Status: Done

## What to build

Expose a minimal read-only Workbench view state through the plugin-facing Workbench service so controls and plugins can react to the active Activity and MainArea view without mutating Workbench internals.

## Acceptance criteria

- [x] `IWorkbenchService` exposes a read-only current state with `ActiveActivityId` and `ActiveMainViewId`.
- [x] `IWorkbenchService` exposes a state-changed notification for changes relevant to plugin UI helpers.
- [x] Plugins cannot access or mutate `WorkbenchState` directly.
- [x] MainArea tabs, Panel dictionaries, Sidebar width, and persistence internals remain hidden.
- [x] Tests cover state snapshot values and state-changed notifications when Activity/MainArea selection changes.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter FullyQualifiedName~WorkbenchViewStateTests`
- Result: passed, 2 tests.
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`
- Result: passed. Existing NU1900 warnings remain in this restricted network environment.

## Notes

- This supports automatic selected-state behavior for `SidebarNavigation` main-view items.
- MainArea opening must still go through `IWorkbenchService.OpenMainViewAsync`.

## Blocked by

- Issue 056
