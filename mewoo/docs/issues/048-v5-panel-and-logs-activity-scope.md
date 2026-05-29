# Issue 048: Panel and Logs Activity Scope

## What to build

Make Panel visibility, height, active panel tab, and Activity-scoped panel tabs part of Activity Workspace state while keeping Logs content global.

## Acceptance criteria

- [ ] Panel visibility and height are stored per Activity Workspace.
- [ ] Activity-scoped panel tabs do not appear in other Activity Workspaces.
- [ ] Logs can be opened from any Activity Workspace.
- [ ] Opening Logs does not make Panel visibility global.
- [ ] Tests cover per-Activity Panel state and global Logs content.

## Blocked by

- Issue 046
