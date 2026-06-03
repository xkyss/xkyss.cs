# Issue 053: Move Plugin Manager to System Section

Status: Done

## What to build

Move the built-in Plugin Manager Activity from the primary ActivityBar section to the bottom system section.

## Acceptance criteria

- [x] Plugin Manager remains implemented as a built-in plugin contribution, not a shell-hardcoded Activity.
- [x] Plugin Manager declares `ActivityBarSection.System`.
- [x] Plugin Manager appears in the bottom ActivityBar system section above Settings.
- [x] Opening Plugin Manager still switches to its own Activity Workspace.
- [x] Plugin Manager Sidebar, MainArea tabs, Panel state, and Activity-scoped StatusBar behavior remain scoped to its Activity Workspace.
- [x] Existing Plugin Manager tests continue to pass.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~MewooBuiltInSystemActivityTests"`

## Notes

- This issue should not change Plugin Manager catalog/install/update behavior.

## Blocked by

- Issue 051
- Issue 052
