# Issue 058: Sidebar Header and Search Controls

Status: Done

## What to build

Add optional `SidebarLayout`, `SidebarHeader`, icon action, More menu, and search-row helpers to `Mewoo.Controls`.

## Acceptance criteria

- [x] `SidebarHeader` renders a left title, an absolutely centered action group, and an optional right More menu.
- [x] Header actions support multiple icon-style buttons.
- [x] Header actions support text icons, `PathGeometry` icons, and custom MewUI elements.
- [x] Header actions require tooltip or accessible label text.
- [x] Title truncates before pushing center actions or More menu out of position.
- [x] More menu uses `...` semantics and does not act as an expandable content panel.
- [x] Actions support async execution, local busy/disabled state, duplicate-click protection, and an `OnActionError` hook.
- [x] `SidebarSearchRow` provides consistent search/filter input visuals and change/binding hooks without owning filtering logic.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter FullyQualifiedName~SidebarHeaderAndSearchControlsTests`
- Result: passed, 2 tests.
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`
- Result: passed. Existing NU1900 warnings remain in this restricted network environment.

## Notes

- Header actions are icon-style only. Text commands belong in the More menu or Sidebar body.
- Search behavior remains plugin-owned.

## Blocked by

- Issue 056
