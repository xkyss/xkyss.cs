# Issue 049: StatusBar Activity and Global Scope

Status: Done

## What to build

Render StatusBar contributions for the current Activity Workspace by default while supporting explicit global StatusBar items.

## Acceptance criteria

- [x] StatusBar contributions are Activity-scoped by default.
- [x] StatusBar contributions can explicitly opt into global scope.
- [x] Switching Activity updates Activity-scoped StatusBar items.
- [x] Global StatusBar items remain visible across Activity switches.
- [x] Runtime updates target the correct Activity-scoped or global item.
- [x] Tests cover scope filtering and runtime updates.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~WorkbenchStatusBarModelTests"`

## Blocked by

- Issue 046
