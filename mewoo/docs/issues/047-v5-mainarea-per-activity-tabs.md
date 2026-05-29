# Issue 047: MainArea Per-Activity Tabs

## What to build

Change MainArea tab state from one global tab stack to per-Activity Workspace tab stacks.

## Acceptance criteria

- [x] Each Activity Workspace has its own open MainArea tabs.
- [x] Switching Activity restores that Activity's active MainArea tab.
- [x] Opening a MainArea view switches to the owning Activity Workspace.
- [x] Closing plugin-owned tabs during unload works across all Activity Workspaces.
- [x] State persistence stores and restores per-Activity MainArea tab stacks.
- [x] Tests cover switching, opening, closing, unload cleanup, and restore behavior.

## Status

Done.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~WorkbenchStateActivityMainViewTests"`
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`
- `dotnet test Mewoo.slnx --no-restore --verbosity minimal`

## Blocked by

- Issue 046
