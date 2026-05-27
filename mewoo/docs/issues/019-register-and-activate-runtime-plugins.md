# Issue 019: Register and Activate Runtime Plugins

## What to build

Wire runtime plugin instances into the existing `MewooPluginHost` registration and activation flow.

## Acceptance criteria

- [x] Runtime plugin contributions render through existing Workbench code.
- [x] Runtime commands register through the existing command registry.
- [x] Runtime plugin activation failures are logged and surfaced like compiled plugin failures.
- [x] Compiled V1 plugins still work unchanged.
- [x] Runtime plugin discovery order is deterministic.

## Blocked by

- Issue 018

## Status

Done

