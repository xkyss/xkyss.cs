# Issue 065: Retire Runtime Diagnostics Activity

Status: Done

## What to build

Remove Runtime Diagnostics as a standalone ActivityBar entry while preserving shared runtime plugin infrastructure for Plugin Manager advanced views.

## Acceptance criteria

- [x] `runtimeDiagnostics.activity` is no longer registered.
- [x] `runtimeDiagnostics.views` is no longer registered.
- [x] `runtimeDiagnostics.home` is no longer registered as a standalone MainView.
- [x] `runtimeDiagnostics.open` is removed or redirected to the Plugin Manager advanced diagnostics entry point.
- [x] ActivityBar no longer shows Runtime Diagnostics as a Primary Activity.
- [x] Plugin Manager remains in the System ActivityBar section above Settings.
- [x] Shared runtime manager, catalog display, logging, and package operation infrastructure remains available.
- [x] Tests that expected a standalone Runtime Diagnostics Activity are updated to the V8 information architecture.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~MewooV5ActivityWorkspaceRoadTestTests|FullyQualifiedName~MewooV6ActivityBarSystemSectionRoadTestTests|FullyQualifiedName~WorkbenchActivityBarModelTests|FullyQualifiedName~MewooBuiltInSystemActivityTests"`
- Result: passed, 8 tests.
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`
- Result: passed. Existing NU1900 warnings remain in this restricted network environment.

## Notes

- This issue retires the user-facing Runtime Diagnostics contribution, not the runtime plugin infrastructure.
- The old `Mewoo.Plugins.RuntimeDiagnostics` project remains buildable for now so Issue 066 can migrate diagnostic UI capability into Plugin Manager advanced views without mixing that migration into this retirement slice.
