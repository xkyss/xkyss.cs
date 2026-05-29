# Issue 042: Update from File Flow

## What to build

Add a Plugin Manager update flow that lets users select a local `.mewoo-plugin` for an installed plugin, validate identity, compare versions, preview risk, and update.

## Acceptance criteria

- [ ] User can choose a `.mewoo-plugin` file from an installed plugin's details view.
- [ ] Package id must match the installed plugin id.
- [ ] Preview shows current version versus package version.
- [ ] Preview shows publisher, trust warning, and permissions.
- [ ] Confirmed update uses the existing staged update operation.
- [ ] Failed update reports that the previous installed package was preserved when applicable.
- [ ] Success and failure are shown in recent operations.
- [ ] Tests cover id mismatch, version comparison, and result mapping outside the UI layer.

## Blocked by

- Issue 041
