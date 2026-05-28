# Issue 031: Define V3 Plugin Package Format

## What to build

Define a local installable plugin package format that wraps a V2 runtime plugin directory into a single distributable artifact.

## Acceptance criteria

- [x] Package extension and archive layout are documented.
- [x] Package contains `mewoo.plugin.json` at a stable location.
- [x] Package contains the plugin assembly and private dependencies.
- [x] Package metadata includes package id, version, display name, and optional publisher fields.
- [x] Invalid package layout failures are diagnosable without modifying the installed plugin root.

## Blocked by

- Issue 030

## Status

Done

## Verification

- `dotnet test tests/Mewoo.Core.Tests/Mewoo.Core.Tests.csproj --no-restore --verbosity minimal`

