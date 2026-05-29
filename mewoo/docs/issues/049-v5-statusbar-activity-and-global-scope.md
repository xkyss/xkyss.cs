# Issue 049: StatusBar Activity and Global Scope

## What to build

Render StatusBar contributions for the current Activity Workspace by default while supporting explicit global StatusBar items.

## Acceptance criteria

- [ ] StatusBar contributions are Activity-scoped by default.
- [ ] StatusBar contributions can explicitly opt into global scope.
- [ ] Switching Activity updates Activity-scoped StatusBar items.
- [ ] Global StatusBar items remain visible across Activity switches.
- [ ] Runtime updates target the correct Activity-scoped or global item.
- [ ] Tests cover scope filtering and runtime updates.

## Blocked by

- Issue 046
