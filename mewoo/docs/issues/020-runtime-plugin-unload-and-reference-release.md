# Issue 020: Runtime Plugin Unload and Reference Release

## What to build

Add runtime plugin unload orchestration around the existing lifecycle and release the plugin load context.

## Acceptance criteria

- [x] Deactivation removes visible contributions.
- [x] Plugin-owned MainArea tabs are closed during unload.
- [x] Plugin lifecycle cleanup is called before load-context unload.
- [x] Loader-held references are released.
- [x] Load-context unload success or residual risk is logged.

## Blocked by

- Issue 019

## Status

Done

