# Issue 040: Plugin Details View

## What to build

Add a dedicated Plugin Details MainArea view for user-facing plugin identity, status, permissions, trust, operations, and actions.

## Acceptance criteria

- [x] Details view opens from Plugin Manager list entries.
- [x] Details show identity, version, publisher, status, trust, permissions, and error summary.
- [x] Details show current-session recent operation results.
- [x] Details expose Enable/Disable, Update from File, Uninstall, and Open Diagnostics.
- [x] Details do not expose Load, Unload, or Reload.
- [x] Details do not show low-level file paths by default.

## Status

Done.

## Verification

- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`

## Blocked by

- Issue 039
