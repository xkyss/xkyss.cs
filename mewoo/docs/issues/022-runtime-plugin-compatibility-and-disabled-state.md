# Issue 022: Runtime Plugin Compatibility and Disabled State

## What to build

Apply `minimumMewooVersion` and disabled-state rules consistently before loading plugin code.

## Acceptance criteria

- [x] Plugins requiring a newer Mewoo build are not loaded.
- [x] Compatibility failures are logged and visible in diagnostics.
- [x] Disabled plugins are discovered but not loaded.
- [x] Disabled-state behavior is stable across restarts.

## Blocked by

- Issue 019

## Status

Done

