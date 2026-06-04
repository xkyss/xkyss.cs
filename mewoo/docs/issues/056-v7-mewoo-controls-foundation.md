# Issue 056: Mewoo.Controls Project and Sidebar Helper Foundation

Status: Planned

## What to build

Add a `Mewoo.Controls` project for optional plugin UI helpers and wire built-in plugin projects so they can use it.

## Acceptance criteria

- [ ] `Mewoo.Controls` exists as a separate project.
- [ ] `Mewoo.Controls` references `Mewoo.Abstractions` and `Aprillz.MewUI`.
- [ ] `Mewoo.Controls` does not reference `Mewoo.Workbench`.
- [ ] Built-in plugins can optionally reference `Mewoo.Controls`.
- [ ] V7 scope is limited to Sidebar helpers and the small common controls required by those helpers.
- [ ] Runtime/local plugin manifests do not gain a required controls dependency declaration.

## Notes

- `Mewoo.Controls` is a helper library, not a required contribution schema.
- V7 common controls include only the primitives needed by Sidebar helpers, such as icon-style action buttons, search input, navigation rows, and empty/placeholder blocks.

## Blocked by

- ADR 0017

