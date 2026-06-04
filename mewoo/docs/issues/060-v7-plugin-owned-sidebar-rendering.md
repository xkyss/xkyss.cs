# Issue 060: Plugin-Owned Sidebar Rendering Boundary

Status: Done

## What to build

Change Workbench Sidebar rendering so plugin-provided Sidebar views are hosted without automatic visible metadata headers.

## Acceptance criteria

- [x] Workbench no longer renders `ViewContainer.Title` above Sidebar content.
- [x] Workbench no longer renders `SidebarView.Title` above Sidebar content.
- [x] `ViewContainer.Title` and `SidebarView.Title` remain contribution metadata.
- [x] Workbench does not add a fallback Sidebar title when a plugin omits one.
- [x] Existing multi-`SidebarView` containers still render their native views in order.
- [x] Tests cover that metadata titles are not inserted as visible Sidebar chrome.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter FullyQualifiedName~WorkbenchSidebarContentRendererTests`
- Result: passed, 1 test.
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`
- Result: passed. Existing NU1900 warnings remain in this restricted network environment.

## Notes

- Visible Sidebar titles are plugin-owned. Plugins may use `Mewoo.Controls.SidebarHeader` or render fully custom content.
- This issue intentionally does not change the ViewContainer contribution model.

## Blocked by

- ADR 0017
