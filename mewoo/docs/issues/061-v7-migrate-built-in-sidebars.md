# Issue 061: Migrate Built-In Sidebars to Mewoo.Controls

Status: Done

## What to build

Migrate built-in plugin Sidebars to the optional `Mewoo.Controls` helpers.

## Acceptance criteria

- [x] Quick Launcher Sidebar uses `SidebarLayout` and related controls where appropriate.
- [x] Plugin Manager Sidebar uses `SidebarLayout` and `SidebarNavigation` where appropriate.
- [x] Runtime Diagnostics Sidebar uses `SidebarLayout` and related controls where appropriate.
- [x] Settings Sidebar uses `SidebarLayout` and `SidebarNavigation` where appropriate.
- [x] MainArea views are not rewritten into a default layout.
- [x] Existing plugin commands and MainArea opening behavior continue to work.
- [x] Built-in Sidebar views no longer duplicate Workbench-rendered titles.

## Verification

- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`
- Result: passed.
- `dotnet test Mewoo.slnx --no-restore --verbosity minimal`
- Result: passed, 97 tests. Existing NU1900 warnings remain in this restricted network environment.

## Notes

- The migration is the product proof for `Mewoo.Controls`; it should remove repeated ad hoc title/search/navigation code without reducing plugin layout freedom.

## Blocked by

- Issue 058
- Issue 059
- Issue 060
