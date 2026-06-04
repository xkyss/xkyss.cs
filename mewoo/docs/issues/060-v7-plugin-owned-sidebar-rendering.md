# Issue 060: Plugin-Owned Sidebar Rendering Boundary

Status: Planned

## What to build

Change Workbench Sidebar rendering so plugin-provided Sidebar views are hosted without automatic visible metadata headers.

## Acceptance criteria

- [ ] Workbench no longer renders `ViewContainer.Title` above Sidebar content.
- [ ] Workbench no longer renders `SidebarView.Title` above Sidebar content.
- [ ] `ViewContainer.Title` and `SidebarView.Title` remain contribution metadata.
- [ ] Workbench does not add a fallback Sidebar title when a plugin omits one.
- [ ] Existing multi-`SidebarView` containers still render their native views in order.
- [ ] Tests cover that metadata titles are not inserted as visible Sidebar chrome.

## Notes

- Visible Sidebar titles are plugin-owned. Plugins may use `Mewoo.Controls.SidebarHeader` or render fully custom content.
- This issue intentionally does not change the ViewContainer contribution model.

## Blocked by

- ADR 0017

