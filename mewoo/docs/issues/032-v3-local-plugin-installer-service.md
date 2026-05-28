# Issue 032: Local Plugin Installer Service

## What to build

Add a core service that installs a V3 plugin package into the runtime plugin root without requiring users to copy folders manually.

## Acceptance criteria

- [x] Installer validates package layout before writing to the plugin root.
- [x] Installer writes into a temporary staging directory first.
- [x] Installer atomically replaces or promotes the package into `%LocalAppData%\Mewoo\Plugins\<pluginId>`.
- [x] Installer refuses package id mismatches.
- [x] Installer returns structured success and failure results for UI diagnostics.
- [x] Unit tests cover successful install, invalid package, id mismatch, and existing plugin replacement.

## Blocked by

- Issue 031

## Status

Done

## Verification

- `dotnet test tests/Mewoo.Core.Tests/Mewoo.Core.Tests.csproj --no-restore --verbosity minimal`

