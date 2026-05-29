# Issue 041: Install from File Flow

## What to build

Add a Plugin Manager install flow that lets users select a local `.mewoo-plugin`, preview it, confirm local-code risk, and install it.

## Acceptance criteria

- [x] User can choose a `.mewoo-plugin` file from Plugin Manager.
- [x] Plugin Manager validates the package before installation.
- [x] Preview shows id, display name, version, publisher, trust warning, and permissions.
- [x] High-risk or undeclared permissions are warned about but not blocked.
- [x] Confirmed install uses the existing staged installer.
- [x] Success and failure are shown in recent operations.
- [x] Tests cover preview mapping and install result mapping outside the UI layer.

## Status

Done.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~MewooPluginInstallFlowTests"`
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`

## Blocked by

- Issue 040
