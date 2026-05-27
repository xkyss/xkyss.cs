# Issue 011: Implement Quick Launcher Actions and Status Feedback

## What to build

Implement Quick Launcher commands and action execution for URL, file, executable, and script targets. Update StatusBar with selected item and last run result.

## Acceptance criteria

- [x] `quickLauncher.open` opens or focuses the launcher MainArea tab.
- [x] `quickLauncher.focusSearch` focuses the launcher search field.
- [x] `quickLauncher.runSelected` runs the selected item.
- [x] `quickLauncher.reload` reloads JSON configuration.
- [x] `quickLauncher.openConfig` opens the configuration target.
- [x] URL targets open in the default browser.
- [x] File, executable, and script targets invoke the expected local target.
- [x] StatusBar updates after selection and execution.

## Blocked by

- Issue 010

## Status

Done

