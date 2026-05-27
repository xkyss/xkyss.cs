# Issue 005: Render Plugin-Contributed Activity, Sidebar, and MainArea View

## What to build

Wire plugin contribution descriptors into the Workbench so one plugin can contribute an ActivityBar item, a Sidebar ViewContainer, and a MainArea view hosted through `IMewooView`.

## Acceptance criteria

- [x] A plugin-contributed ActivityBar item renders in the ActivityBar.
- [x] Selecting the ActivityBar item shows its ViewContainer in the Sidebar.
- [x] A plugin-contributed MainArea view can be hosted.
- [x] Unsupported native views render an error view instead of crashing the shell.
- [x] Workbench selection is based on ActivityId, not PluginId.

## Blocked by

- Issue 003
- Issue 004

## Status

Done

