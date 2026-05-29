# Issue 038: Plugin Manager Catalog Model

## What to build

Create a Core-layer catalog model for Plugin Manager that maps runtime plugin data, broken installed directories, trust metadata, permissions, search, and filters into a user-facing shape.

## Acceptance criteria

- [ ] Catalog model distinguishes enabled, disabled, loaded, failed, incompatible, and broken entries.
- [ ] Catalog model includes identity, version, publisher, trust, permissions, and user-facing status labels.
- [ ] Broken installed directories are included when manifest or assembly data cannot be read.
- [ ] Search supports plugin name, id, and publisher.
- [ ] Filters support all, enabled, disabled, failed, incompatible, and broken.
- [ ] Tests cover state mapping, broken entries, search, and filters.

## Blocked by

- Issue 037
