# Issue 039: Plugin Manager Activity and List UI

## What to build

Add the first-class `Plugins` ActivityBar module and list UI for scanning installed local plugins.

## Acceptance criteria

- [x] ActivityBar shows a `Plugins` entry separate from Runtime.
- [x] Sidebar or main view shows installed plugin entries from the Plugin Manager catalog model.
- [x] List supports search and status filters.
- [x] Empty state guides the user to install a local `.mewoo-plugin`.
- [x] List item fields include name, id, version, publisher, status, and permission summary.
- [x] Plugin Manager list does not show manifest path, assembly path, or install directory by default.

## Status

Done.

## Verification

- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`

## Blocked by

- Issue 038
