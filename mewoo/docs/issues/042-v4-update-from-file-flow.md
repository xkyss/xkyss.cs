# Issue 042: Update from File Flow

## What to build

Add a Plugin Manager update flow that lets users select a local `.mewoo-plugin` for an installed plugin, validate identity, compare versions, preview risk, and update.

## Acceptance criteria

- [x] User can choose a `.mewoo-plugin` file from an installed plugin's details view.
- [x] Package id must match the installed plugin id.
- [x] Preview shows current version versus package version.
- [x] Preview shows publisher, trust warning, and permissions.
- [x] Confirmed update uses the existing staged update operation.
- [x] Failed update reports that the previous installed package was preserved when applicable.
- [x] Success and failure are shown in recent operations.
- [x] Tests cover id mismatch, version comparison, and result mapping outside the UI layer.

## Status

Done.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~MewooPluginInstallFlowTests"`
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`
- `dotnet test Mewoo.slnx --no-restore --verbosity minimal`

## Blocked by

- Issue 041
