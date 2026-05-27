# Issue 025: Add Runtime Diagnostics Operations

## What to build

Add basic operation buttons to Runtime Diagnostics for runtime plugin unload, reload, and disabled-state editing.

## Acceptance criteria

- [ ] Loaded runtime plugins expose an Unload action.
- [ ] Discovered runtime plugins expose a Reload action.
- [ ] Disabled plugins are visible with an Enable action.
- [ ] Enabled plugins expose a Disable action.
- [ ] Operation results are logged.
- [ ] Operation failures do not crash the shell.
- [ ] Diagnostics UI refreshes after each operation.

## Blocked by

- Issue 024

