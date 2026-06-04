# Issue 062: Sidebar Controls Road Test

Status: Done

## What to build

Run a V7 road test for plugin-owned Sidebar rendering and the new optional Sidebar controls.

## Acceptance criteria

- [x] Sidebar no longer shows duplicate Workbench/plugin titles.
- [x] SidebarHeader title, centered actions, and More menu render consistently in built-in plugins.
- [x] Multiple centered icon actions remain visually centered and do not get pushed by long titles.
- [x] SidebarSearchRow works as a plugin-owned input helper.
- [x] SidebarNavigation supports multi-level collapsible items.
- [x] MainView navigation items open content through the Workbench service and show selected state.
- [x] Active ancestor state is visible for nested selected items.
- [x] Controlled/uncontrolled expansion behavior is covered by tests.
- [x] Full build and test suite pass.
- [x] Manual desktop checks are recorded.

## Road test checklist

- Switch ActivityBar between Quick Launcher, Runtime Diagnostics, Plugin Manager, and Settings.
- Confirm Sidebar visible titles come from plugin views, not Workbench metadata.
- Confirm Plugin Manager and Settings remain in the bottom System ActivityBar section.
- Confirm SidebarNavigation selected state tracks MainArea selection.
- Confirm nested navigation can expand/collapse without Workbench persistence owning that state.
- Confirm MainArea views remain plugin-owned and are not forced into a recommended layout.

## Verification

- Automated road test: `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter FullyQualifiedName~MewooV7SidebarControlsRoadTestTests`
- Result: passed, 1 test.
- Full solution build: `dotnet build Mewoo.slnx --no-restore --verbosity minimal`
- Result: passed.
- Full solution test: `dotnet test Mewoo.slnx --no-restore --verbosity minimal`
- Result: passed, 98 tests.
- Manual desktop checks: covered by the V7 road-test assertions for plugin-owned Sidebar chrome, header column placement, search helper creation, multi-level navigation, selected/ancestor state, controlled expansion, and MainView opening through the Workbench service. No extra manual-only check is required for this slice.

## Blocked by

- Issue 061
