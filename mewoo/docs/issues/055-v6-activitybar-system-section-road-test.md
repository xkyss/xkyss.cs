# Issue 055: ActivityBar System Section Road Test

## What to build

Run a V6 road test for ActivityBar system-section behavior across Primary Activities, Plugin Manager, and Settings.

## Acceptance criteria

- [ ] Quick Launcher and Runtime Diagnostics remain in the Primary ActivityBar section.
- [ ] Plugin Manager and Settings render in the bottom System ActivityBar section.
- [ ] Plugin Manager appears above Settings.
- [ ] Switching between Primary and System Activities switches the full Activity Workspace.
- [ ] Runtime/local plugin attempts to declare a system-section Activity fail with a clear diagnostic.
- [ ] Full build and test suite pass.
- [ ] Manual desktop checks are recorded.

## Road test checklist

- Switch ActivityBar between Quick Launcher, Runtime Diagnostics, Plugin Manager, and Settings.
- Confirm each Activity has its own Sidebar and MainArea tab state.
- Confirm Plugin Manager and Settings stay pinned at the bottom of ActivityBar.
- Confirm Settings is the bottom-most system item.
- Confirm Plugin Manager remains usable for install/update/details flows.

## Blocked by

- Issue 053
- Issue 054
