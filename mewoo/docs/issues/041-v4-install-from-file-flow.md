# Issue 041: Install from File Flow

## What to build

Add a Plugin Manager install flow that lets users select a local `.mewoo-plugin`, preview it, confirm local-code risk, and install it.

## Acceptance criteria

- [ ] User can choose a `.mewoo-plugin` file from Plugin Manager.
- [ ] Plugin Manager validates the package before installation.
- [ ] Preview shows id, display name, version, publisher, trust warning, and permissions.
- [ ] High-risk or undeclared permissions are warned about but not blocked.
- [ ] Confirmed install uses the existing staged installer.
- [ ] Success and failure are shown in recent operations.
- [ ] Tests cover preview mapping and install result mapping outside the UI layer.

## Blocked by

- Issue 040
