# Issue 028: Runtime Plugin Failure UX Pass

## What to build

Improve diagnostics display for invalid manifests, missing assemblies, entry point errors, activation errors, disabled plugins, and incompatible plugins.

## Acceptance criteria

- [x] Failure category is visible.
- [x] Short failure message is visible.
- [x] Manifest path is copyable or plainly visible.
- [x] Assembly path is copyable or plainly visible.
- [x] Logs panel can be opened from diagnostics.
- [x] Empty state is clear when no runtime plugins are discovered.

## Notes

- Runtime plugin statuses now carry an issue category and short message for compatibility, assembly, entry point, registration, activation, disabled, and unload states.
- Runtime manifest discovery failures are retained separately so invalid manifests can appear in diagnostics without crashing discovery.
- Runtime Diagnostics exposes category/message/path details and an Open Logs action.
- Tests were added for discovery issue recording, assembly load issue details, entry point issue details, disabled/compatibility categories, and activation failure status.

## Blocked by

- Slice 021
