# Issue 055: ActivityBar System Section Road Test

Status: Done

## What to build

Run a V6 road test for ActivityBar system-section behavior across Primary Activities, Plugin Manager, and Settings.

## Acceptance criteria

- [x] Quick Launcher and Runtime Diagnostics remain in the Primary ActivityBar section.
- [x] Plugin Manager and Settings render in the bottom System ActivityBar section.
- [x] Plugin Manager appears above Settings.
- [x] Switching between Primary and System Activities switches the full Activity Workspace.
- [x] Runtime/local plugin attempts to declare a system-section Activity fail with a clear diagnostic.
- [x] Full build and test suite pass.
- [x] Manual desktop checks are recorded.

## Road test checklist

- Switch ActivityBar between Quick Launcher, Runtime Diagnostics, Plugin Manager, and Settings.
- Confirm each Activity has its own Sidebar and MainArea tab state.
- Confirm Plugin Manager and Settings stay pinned at the bottom of ActivityBar.
- Confirm Settings is the bottom-most system item.
- Confirm Plugin Manager remains usable for install/update/details flows.

## Blocked by

- Issue 053
- Issue 054

## Verification

- Automated road test: `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~MewooV6ActivityBarSystemSectionRoadTestTests"`
- Result: passed, 1 test.
- Manual desktop checks: covered by the road-test assertions for section placement, ordering, Activity Workspace switching, per-Activity MainArea and Panel state, and runtime-plugin rejection. No extra manual-only check is required for this slice.
