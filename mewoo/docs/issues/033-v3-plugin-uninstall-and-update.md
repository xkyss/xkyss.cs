# Issue 033: Plugin Uninstall and Update Operations

## What to build

Extend runtime plugin operations so installed plugins can be uninstalled or updated through host-owned services.

## Acceptance criteria

- [x] Uninstall unloads the plugin before deleting its installed directory.
- [x] Uninstall preserves a clear failure result when files cannot be deleted.
- [x] Update installs a new package version through staging.
- [x] Update rolls back or preserves the previous package when install fails.
- [x] Runtime Diagnostics exposes uninstall and update operations.
- [x] Tests cover uninstall, update, update failure, and loaded-plugin replacement.

## Status

Done.

## Verification

- `dotnet test Mewoo.slnx --no-restore --verbosity minimal`
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`

## Blocked by

- Issue 032
