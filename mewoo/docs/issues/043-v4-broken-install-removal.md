# Issue 043: Broken Install Removal

## What to build

Allow Plugin Manager to show and remove broken installed plugin directories.

## Acceptance criteria

- [x] Broken entries appear for missing manifest, invalid manifest, invalid id, and missing assembly cases.
- [x] Broken entries show a user-facing error summary.
- [x] User can remove a broken installed plugin directory from Plugin Manager.
- [x] Removal reports success or failure in recent operations.
- [x] Tests cover broken install discovery and removal failure.

## Status

Done.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~MewooPluginManagerCatalogTests|FullyQualifiedName~MewooPluginPackageOperationsTests"`
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`
- `dotnet test Mewoo.slnx --no-restore --verbosity minimal`

## Blocked by

- Issue 038
