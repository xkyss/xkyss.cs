# Issue 066: Plugin Manager Advanced Diagnostic Views

Status: Done

## What to build

Move runtime diagnostic UI capability into Plugin Manager advanced Sidebar entries and MainArea views.

## Acceptance criteria

- [x] Plugin Manager Sidebar includes an `Advanced` diagnostics group.
- [x] The `Advanced` group is collapsed by default.
- [x] Runtime or discovery issues can surface counts or warning state on the advanced group or related entries.
- [x] Plugin Manager contributes a `pluginManager.runtimeStatus` MainView for runtime plugin status.
- [x] Plugin Manager contributes a `pluginManager.discoveryIssues` MainView for discovery issues.
- [x] Plugin Manager contributes a `pluginManager.operationLog` MainView for package operations and relevant runtime logs.
- [x] `Open Diagnostics` from Plugin Details opens or focuses the relevant Plugin Manager advanced view.
- [x] Advanced views may show developer-facing paths, runtime states, low-level messages, and diagnostic actions.
- [x] Management views keep product-facing language and do not expose raw diagnostic density by default.

## Verification

- Added `PluginManagerContributesAdvancedDiagnosticViews` and extended Sidebar category coverage for the collapsed `Advanced` group.
- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~MewooBuiltInSystemActivityTests|FullyQualifiedName~PluginManager|FullyQualifiedName~PluginPackageOperations|FullyQualifiedName~SidebarNavigationControlsTests"`
- Result: passed, 26 tests.
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`
- Result: passed. Existing NU1900 warnings remain in this restricted network environment.

## Notes

- Advanced diagnostics live inside the `Plugins` System Activity Workspace.
- MainArea remains Workbench-hosted MainViews; do not add an internal mode switch or TabControl.
