# Issue 064: Plugin Manager Sidebar Category Navigation

Status: Done

## What to build

Move Plugin Manager management categories into its Sidebar navigation and remove duplicate category controls from the Plugin Manager home MainArea view.

## Acceptance criteria

- [x] Plugin Manager Sidebar has a management section with `All`, `Enabled`, `Disabled`, and `Needs Attention`.
- [x] `Needs Attention` includes failed, incompatible, broken, or otherwise action-worthy plugin entries.
- [x] Selecting a management category opens or focuses `pluginManager.home`.
- [x] `pluginManager.home` renders the selected category's plugin list.
- [x] Plugin Manager home does not add an internal category `TabControl` or duplicate category button row.
- [x] Plugin Details remains a dedicated MainArea view opened from a selected plugin row.
- [x] Install and update previews remain dedicated MainArea views rather than modal dialogs.

## Verification

- Added `PluginManagerSidebarCategoriesDriveHomeViewCategory`.
- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~MewooBuiltInSystemActivityTests|FullyQualifiedName~PluginManager|FullyQualifiedName~PluginPackageOperations"`
- Result: passed, 22 tests.
- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter FullyQualifiedName~SidebarNavigationControlsTests`
- Result: passed, 3 tests.
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`
- Result: passed. Existing NU1900 and MSTEST0037 warnings remain.

## Notes

- Sidebar navigation owns category selection; Workbench only owns the surrounding Activity Workspace shell state.
