# Issue 067: Unified Plugin Manager Workspace Road Test

Status: Done

## What to build

Run an end-to-end V8 road test for the unified Plugin Manager workspace after Runtime Diagnostics is folded into Plugin Manager.

## Acceptance criteria

- [x] Plugin Manager appears in the ActivityBar System section above Settings.
- [x] Runtime Diagnostics no longer appears as an ActivityBar entry.
- [x] Plugin Manager Sidebar management categories drive the home plugin list.
- [x] Plugin Manager home does not include an internal category TabControl.
- [x] Plugin Details opens as a dedicated MainArea view.
- [x] Install and update previews open as dedicated MainArea views.
- [x] Advanced diagnostics are available from Plugin Manager Sidebar and Plugin Details.
- [x] Advanced diagnostics default collapsed behavior and issue counts are covered.
- [x] Runtime status, discovery issues, operation log, install, update, enable, disable, uninstall, and remove broken install flows remain usable.

## Verification

- Added `MewooV8UnifiedPluginManagerWorkspaceRoadTestTests`.
- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter FullyQualifiedName~MewooV8UnifiedPluginManagerWorkspaceRoadTestTests`
- Result: passed, 1 test.
- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~MewooV8UnifiedPluginManagerWorkspaceRoadTestTests|FullyQualifiedName~PluginManager|FullyQualifiedName~RuntimePlugin|FullyQualifiedName~ActivityWorkspace|FullyQualifiedName~ActivityBarSystem|FullyQualifiedName~Sidebar"`
- Result: passed, 38 tests.
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`
- Result: passed. Existing NU1900 warnings remain in this restricted network environment.

## Notes

- This issue should verify behavior and architecture together; it should not introduce new Plugin Manager product features.
