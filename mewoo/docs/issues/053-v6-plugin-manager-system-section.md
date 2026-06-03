# Issue 053: Move Plugin Manager to System Section

## What to build

Move the built-in Plugin Manager Activity from the primary ActivityBar section to the bottom system section.

## Acceptance criteria

- [ ] Plugin Manager remains implemented as a built-in plugin contribution, not a shell-hardcoded Activity.
- [ ] Plugin Manager declares `ActivityBarSection.System`.
- [ ] Plugin Manager appears in the bottom ActivityBar system section above Settings.
- [ ] Opening Plugin Manager still switches to its own Activity Workspace.
- [ ] Plugin Manager Sidebar, MainArea tabs, Panel state, and Activity-scoped StatusBar behavior remain scoped to its Activity Workspace.
- [ ] Existing Plugin Manager tests continue to pass.

## Notes

- This issue should not change Plugin Manager catalog/install/update behavior.

## Blocked by

- Issue 051
- Issue 052
