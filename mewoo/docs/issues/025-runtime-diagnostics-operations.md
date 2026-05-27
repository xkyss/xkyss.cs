# Issue 025: Add Runtime Diagnostics Operations

## What to build

Add basic operation buttons to Runtime Diagnostics for runtime plugin unload, reload, and disabled-state editing.

## Acceptance criteria

- [x] Loaded runtime plugins expose an Unload action.
- [x] Discovered runtime plugins expose a Reload action.
- [x] Disabled plugins are visible with an Enable action.
- [x] Enabled plugins expose a Disable action.
- [x] Operation results are logged.
- [x] Operation failures do not crash the shell.
- [x] Diagnostics UI refreshes after each operation.

## Blocked by

- Issue 024

## Status

Done

## Verification

- `dotnet test Mewoo.slnx`
- `dotnet build Mewoo.slnx`
