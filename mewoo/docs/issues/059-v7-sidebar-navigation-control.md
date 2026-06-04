# Issue 059: SidebarNavigation Multi-Level Collapsible Control

Status: Done

## What to build

Add `SidebarNavigation` as an optional multi-level collapsible Sidebar navigation helper.

## Acceptance criteria

- [x] `SidebarNavigation` supports multiple nesting levels.
- [x] Navigation nodes can be collapsible.
- [x] Navigation items can execute a command, open a MainArea view, or run a custom action.
- [x] MainArea view items open content through `IWorkbenchService.OpenMainViewAsync`.
- [x] MainArea view items can automatically show selected state from read-only Workbench state.
- [x] Ancestor nodes of the selected item show a contains-active state.
- [x] Uncontrolled navigation can auto-expand ancestors of the selected item.
- [x] Controlled navigation reports expansion changes without mutating plugin-owned expanded IDs.
- [x] Expanded/collapsed state is owned by the plugin or helper view instance, not Workbench persistence.
- [x] Search/filtering is not automatically bound to `SidebarSearchRow`; plugins can wire filtering themselves.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter FullyQualifiedName~SidebarNavigationControlsTests`
- Result: passed, 3 tests.
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`
- Result: passed. Existing NU1900 warnings remain in this restricted network environment.

## Notes

- `SidebarNavigation` replaces the earlier menu naming because V7 needs multi-level and collapsible navigation.
- This is not a full generic file-tree control.

## Blocked by

- Issue 057
- Issue 058
