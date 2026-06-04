# Issue 062: Sidebar Controls Road Test

Status: Planned

## What to build

Run a V7 road test for plugin-owned Sidebar rendering and the new optional Sidebar controls.

## Acceptance criteria

- [ ] Sidebar no longer shows duplicate Workbench/plugin titles.
- [ ] SidebarHeader title, centered actions, and More menu render consistently in built-in plugins.
- [ ] Multiple centered icon actions remain visually centered and do not get pushed by long titles.
- [ ] SidebarSearchRow works as a plugin-owned input helper.
- [ ] SidebarNavigation supports multi-level collapsible items.
- [ ] MainView navigation items open content through the Workbench service and show selected state.
- [ ] Active ancestor state is visible for nested selected items.
- [ ] Controlled/uncontrolled expansion behavior is covered by tests.
- [ ] Full build and test suite pass.
- [ ] Manual desktop checks are recorded.

## Road test checklist

- Switch ActivityBar between Quick Launcher, Runtime Diagnostics, Plugin Manager, and Settings.
- Confirm Sidebar visible titles come from plugin views, not Workbench metadata.
- Confirm Plugin Manager and Settings remain in the bottom System ActivityBar section.
- Confirm SidebarNavigation selected state tracks MainArea selection.
- Confirm nested navigation can expand/collapse without Workbench persistence owning that state.
- Confirm MainArea views remain plugin-owned and are not forced into a recommended layout.

## Blocked by

- Issue 061

