# Issue 065: Retire Runtime Diagnostics Activity

Status: Planned

## What to build

Remove Runtime Diagnostics as a standalone ActivityBar entry while preserving shared runtime plugin infrastructure for Plugin Manager advanced views.

## Acceptance criteria

- [ ] `runtimeDiagnostics.activity` is no longer registered.
- [ ] `runtimeDiagnostics.views` is no longer registered.
- [ ] `runtimeDiagnostics.home` is no longer registered as a standalone MainView.
- [ ] `runtimeDiagnostics.open` is removed or redirected to the Plugin Manager advanced diagnostics entry point.
- [ ] ActivityBar no longer shows Runtime Diagnostics as a Primary Activity.
- [ ] Plugin Manager remains in the System ActivityBar section above Settings.
- [ ] Shared runtime manager, catalog display, logging, and package operation infrastructure remains available.
- [ ] Tests that expected a standalone Runtime Diagnostics Activity are updated to the V8 information architecture.

## Verification

- Run ActivityBar section tests and V5/V6 Activity Workspace road-test coverage updated for V8.
- Build the solution.

## Notes

- This issue retires the user-facing Runtime Diagnostics contribution, not the runtime plugin infrastructure.
