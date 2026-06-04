# Issue 059: SidebarNavigation Multi-Level Collapsible Control

Status: Planned

## What to build

Add `SidebarNavigation` as an optional multi-level collapsible Sidebar navigation helper.

## Acceptance criteria

- [ ] `SidebarNavigation` supports multiple nesting levels.
- [ ] Navigation nodes can be collapsible.
- [ ] Navigation items can execute a command, open a MainArea view, or run a custom action.
- [ ] MainArea view items open content through `IWorkbenchService.OpenMainViewAsync`.
- [ ] MainArea view items can automatically show selected state from read-only Workbench state.
- [ ] Ancestor nodes of the selected item show a contains-active state.
- [ ] Uncontrolled navigation can auto-expand ancestors of the selected item.
- [ ] Controlled navigation reports expansion changes without mutating plugin-owned expanded IDs.
- [ ] Expanded/collapsed state is owned by the plugin or helper view instance, not Workbench persistence.
- [ ] Search/filtering is not automatically bound to `SidebarSearchRow`; plugins can wire filtering themselves.

## Notes

- `SidebarNavigation` replaces the earlier menu naming because V7 needs multi-level and collapsible navigation.
- This is not a full generic file-tree control.

## Blocked by

- Issue 057
- Issue 058

