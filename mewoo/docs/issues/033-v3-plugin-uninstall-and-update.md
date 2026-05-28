# Issue 033: Plugin Uninstall and Update Operations

## What to build

Extend runtime plugin operations so installed plugins can be uninstalled or updated through host-owned services.

## Acceptance criteria

- [ ] Uninstall unloads the plugin before deleting its installed directory.
- [ ] Uninstall preserves a clear failure result when files cannot be deleted.
- [ ] Update installs a new package version through staging.
- [ ] Update rolls back or preserves the previous package when install fails.
- [ ] Runtime Diagnostics exposes uninstall and update operations.
- [ ] Tests cover uninstall, update, update failure, and loaded-plugin replacement.

## Blocked by

- Issue 032

