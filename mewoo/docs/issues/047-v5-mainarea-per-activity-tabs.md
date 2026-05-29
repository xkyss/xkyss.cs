# Issue 047: MainArea Per-Activity Tabs

## What to build

Change MainArea tab state from one global tab stack to per-Activity Workspace tab stacks.

## Acceptance criteria

- [ ] Each Activity Workspace has its own open MainArea tabs.
- [ ] Switching Activity restores that Activity's active MainArea tab.
- [ ] Opening a MainArea view switches to the owning Activity Workspace.
- [ ] Closing plugin-owned tabs during unload works across all Activity Workspaces.
- [ ] State persistence stores and restores per-Activity MainArea tab stacks.
- [ ] Tests cover switching, opening, closing, unload cleanup, and restore behavior.

## Blocked by

- Issue 046
