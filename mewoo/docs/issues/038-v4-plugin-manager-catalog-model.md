# Issue 038: Plugin Manager Catalog Model

## What to build

Create a Core-layer catalog model for Plugin Manager that maps runtime plugin data, broken installed directories, trust metadata, permissions, search, and filters into a user-facing shape.

## Acceptance criteria

- [x] Catalog model distinguishes enabled, disabled, loaded, failed, incompatible, and broken entries.
- [x] Catalog model includes identity, version, publisher, trust, permissions, and user-facing status labels.
- [x] Broken installed directories are included when manifest or assembly data cannot be read.
- [x] Search supports plugin name, id, and publisher.
- [x] Filters support all, enabled, disabled, failed, incompatible, and broken.
- [x] Tests cover state mapping, broken entries, search, and filters.

## Status

Done.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~MewooPluginManagerCatalogTests"`

## Blocked by

- Issue 037
