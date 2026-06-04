# Issue 058: Sidebar Header and Search Controls

Status: Planned

## What to build

Add optional `SidebarLayout`, `SidebarHeader`, icon action, More menu, and search-row helpers to `Mewoo.Controls`.

## Acceptance criteria

- [ ] `SidebarHeader` renders a left title, an absolutely centered action group, and an optional right More menu.
- [ ] Header actions support multiple icon-style buttons.
- [ ] Header actions support text icons, `PathGeometry` icons, and custom MewUI elements.
- [ ] Header actions require tooltip or accessible label text.
- [ ] Title truncates before pushing center actions or More menu out of position.
- [ ] More menu uses `...` semantics and does not act as an expandable content panel.
- [ ] Actions support async execution, local busy/disabled state, duplicate-click protection, and an `OnActionError` hook.
- [ ] `SidebarSearchRow` provides consistent search/filter input visuals and change/binding hooks without owning filtering logic.

## Notes

- Header actions are icon-style only. Text commands belong in the More menu or Sidebar body.
- Search behavior remains plugin-owned.

## Blocked by

- Issue 056

