# Issue 032: Local Plugin Installer Service

## What to build

Add a core service that installs a V3 plugin package into the runtime plugin root without requiring users to copy folders manually.

## Acceptance criteria

- [ ] Installer validates package layout before writing to the plugin root.
- [ ] Installer writes into a temporary staging directory first.
- [ ] Installer atomically replaces or promotes the package into `%LocalAppData%\Mewoo\Plugins\<pluginId>`.
- [ ] Installer refuses package id mismatches.
- [ ] Installer returns structured success and failure results for UI diagnostics.
- [ ] Unit tests cover successful install, invalid package, id mismatch, and existing plugin replacement.

## Blocked by

- Issue 031

