# ADR 0017: Keep Sidebar Interiors Plugin-Owned with Optional Controls

Date: 2026-06-04

## Status

Accepted

## Context

V5 made Sidebar and MainArea part of the active Activity Workspace, and V6 added a system ActivityBar section without changing that model. The current Workbench still automatically renders `ViewContainer.Title` and `SidebarView.Title` above plugin Sidebar views, while plugins often render their own titles, subtitles, search fields, and navigation rows inside the view. This creates repeated headers and makes each built-in plugin reimplement the same small Sidebar layout patterns.

## Decision

Mewoo will keep Sidebar and MainArea interiors plugin-owned. Workbench will stop automatically rendering `ViewContainer.Title` and `SidebarView.Title` as visible Sidebar headers; those titles remain metadata for diagnostics, accessibility, and future tooling. Plugins that want a standard Sidebar shape can opt into a new `Mewoo.Controls` library.

`Mewoo.Controls` will provide optional Sidebar helpers and small common controls used by those helpers. The first recommended Sidebar shape is:

```text
Title        [action][action]      [...]
[ optional search/filter row           ]
multi-level collapsible navigation/body
```

Header actions are icon-style, support multiple buttons, and are centered relative to the Sidebar width. The right-side `...` is a More menu, not an expandable content panel. `SidebarNavigation` supports multi-level collapsible navigation, but its expanded/collapsed state belongs to the plugin or helper view instance, not to Workbench persistence.

MainArea will not get a default or recommended layout in this decision. Each plugin owns its MainArea view composition.

## Consequences

- The duplicate Sidebar title/subtitle problem is fixed at the Workbench boundary instead of by special-casing individual plugins.
- Built-in and runtime plugins may use `Mewoo.Controls`, but they can also keep fully custom Sidebar views.
- Workbench remains responsible for Activity Workspace switching, contribution ownership, and MainArea tab lifecycle, while plugins remain responsible for Sidebar interior layout and behavior.
- `ViewContainer.Title` and `SidebarView.Title` no longer guarantee visible text in the Sidebar.

