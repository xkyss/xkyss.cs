# Issue 007: Implement Plugin Lifecycle and Contribution Revocation

## What to build

Implement V1 plugin lifecycle states and shell orchestration for registration, activation, deactivation, unload, dispose, and failure handling.

## Acceptance criteria

- [x] Plugins move through Created, Registered, Activated, Deactivated, Unloaded, and Disposed states.
- [x] Any lifecycle failure moves the plugin to Failed.
- [x] Register failure makes the plugin invisible.
- [x] Activate failure hides plugin contributions and records the error.
- [x] Deactivated plugin contributions are hidden by default.
- [x] Unloading a plugin closes its owned MainArea tabs.
- [x] Cleanup failures are recorded without blocking shell cleanup.

## Blocked by

- Issue 004
- Issue 006

## Status

Done

