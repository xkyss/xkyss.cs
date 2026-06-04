# Issue 061: Migrate Built-In Sidebars to Mewoo.Controls

Status: Planned

## What to build

Migrate built-in plugin Sidebars to the optional `Mewoo.Controls` helpers.

## Acceptance criteria

- [ ] Quick Launcher Sidebar uses `SidebarLayout` and related controls where appropriate.
- [ ] Plugin Manager Sidebar uses `SidebarLayout` and `SidebarNavigation` where appropriate.
- [ ] Runtime Diagnostics Sidebar uses `SidebarLayout` and related controls where appropriate.
- [ ] Settings Sidebar uses `SidebarLayout` and `SidebarNavigation` where appropriate.
- [ ] MainArea views are not rewritten into a default layout.
- [ ] Existing plugin commands and MainArea opening behavior continue to work.
- [ ] Built-in Sidebar views no longer duplicate Workbench-rendered titles.

## Notes

- The migration is the product proof for `Mewoo.Controls`; it should remove repeated ad hoc title/search/navigation code without reducing plugin layout freedom.

## Blocked by

- Issue 058
- Issue 059
- Issue 060

