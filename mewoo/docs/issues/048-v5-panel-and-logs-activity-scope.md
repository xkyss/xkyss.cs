# Issue 048: Panel and Logs Activity Scope

Status: Done

## What to build

Make Panel visibility, height, active panel tab, and Activity-scoped panel tabs part of Activity Workspace state while keeping Logs content global.

## Acceptance criteria

- [x] Panel visibility and height are stored per Activity Workspace.
- [x] Activity-scoped panel tabs do not appear in other Activity Workspaces.
- [x] Logs can be opened from any Activity Workspace.
- [x] Opening Logs does not make Panel visibility global.
- [x] Tests cover per-Activity Panel state and global Logs content.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~WorkbenchStatePanelScopeTests"`

## Blocked by

- Issue 046
