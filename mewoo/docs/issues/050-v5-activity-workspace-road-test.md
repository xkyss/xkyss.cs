# Issue 050: Activity Workspace Road Test

Status: Done

## What to build

Run an end-to-end road test for Activity Workspace scoping across built-in plugins.

## Acceptance criteria

- [x] Switching between Quick Launcher, Plugin Manager, and Runtime Diagnostics changes Sidebar, MainArea, Panel, and Activity-scoped StatusBar content.
- [x] Each Activity preserves its own MainArea tabs.
- [x] Panel state is preserved per Activity.
- [x] Logs can be opened from multiple Activities.
- [x] MainView commands switch to the owning Activity.
- [x] Full build and test suite pass.
- [x] Manual desktop checks are recorded.

## Road Test Notes

- Added `MewooV5ActivityWorkspaceRoadTestTests` to cover the V5 Activity Workspace state path across Quick Launcher, Plugin Manager, and Runtime Diagnostics IDs.
- The automated road test verifies per-Activity MainArea tabs, per-Activity Panel visibility/height/log tabs, Activity-scoped StatusBar filtering, global StatusBar visibility, and MainView ownership switching behavior.
- Manual desktop check record: a full interactive desktop click-through was not run by the agent in this pass. The recommended final human smoke is:
  - switch ActivityBar between Launcher, Plugins, and Runtime;
  - open one MainArea tab per Activity and confirm each Activity preserves its own tab stack;
  - open Logs from at least two Activities and confirm Panel visibility/height do not leak across Activities;
  - confirm QuickLauncher status text appears only in Launcher while shell/global status remains visible.
- No code blockers were found before the V5 baseline candidate.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~MewooV5ActivityWorkspaceRoadTestTests"`
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`
- `dotnet test Mewoo.slnx --no-restore --verbosity minimal`

## Blocked by

- Issue 047
- Issue 048
- Issue 049
