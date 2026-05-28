# Issue 034: Plugin Catalog Metadata UI

## What to build

Show installed plugin package metadata in Runtime Diagnostics so local packages feel manageable rather than just loadable.

## Acceptance criteria

- [x] Runtime Diagnostics shows installed package version and publisher metadata when available.
- [x] Runtime Diagnostics distinguishes discovered, installed, disabled, incompatible, failed, and loaded states.
- [x] Runtime Diagnostics shows install/update/uninstall results.
- [x] Empty state explains where local packages can be installed from.
- [x] Tests cover catalog state mapping outside the UI layer.

## Status

Done.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal`
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`

## Blocked by

- Issue 033
