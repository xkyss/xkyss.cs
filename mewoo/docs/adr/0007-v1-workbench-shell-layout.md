# ADR 0007: Define the V1 Workbench Shell Layout

## Status

Accepted

## Context

Mewoo is a VSCode-like personal desktop shell built on MewUI. The shell needs a stable visual structure before plugin APIs and workbench services are implemented. MewUI includes examples for borderless windows, which should be referenced before implementing the custom title bar.

## Decision

Mewoo V1 uses a borderless main window with a custom title bar.

The top-level layout is:

```text
Root
├─ CustomTitleBar
├─ Workbench
│  ├─ ActivityBar
│  ├─ Sidebar
│  ├─ MainArea
│  │  ├─ MainAreaTabs
│  │  └─ ActiveMainView
│  ├─ Panel
│  └─ StatusBar
```

The custom title bar contains:

- Left: app icon, app name, and version.
- Left adjacent: function menus such as File, View, and Help.
- Center: current MainArea or active workbench title.
- Right: functional icons such as theme switching and always-on-top.
- Far right: minimize, maximize/restore, and close buttons.

Workbench region decisions:

- ActivityBar uses icon-only items with hover tooltips.
- Selecting an ActivityBar item shows its corresponding `ViewContainer` in the Sidebar.
- A plugin may contribute multiple ActivityBar items.
- MainArea supports multiple tabs.
- MainArea does not support split editor groups in V1.
- Sidebar and Panel can be resized.
- Panel exists in V1 but is hidden by default.

Suggested initial dimensions:

```text
TitleBar: 34px
ActivityBar: 48px
Sidebar: 260px, min 180px, max 420px
Panel: 260px height, min 120px, max 50% window height
StatusBar: 24px
MainAreaTabs: 35px
```

## Consequences

Mewoo owns window chrome behavior instead of relying on the operating system title bar. This makes the app feel closer to VSCode and allows shell-level actions in the title bar, but it requires careful implementation of dragging, double-click maximize/restore, system window controls, hit testing, resizing, and platform-specific edge cases.

The title bar becomes part of the workbench contract. It needs access to app identity, version, active title, menu contributions, title-bar command contributions, theme state, always-on-top state, and window control commands.

Using a borderless window means the MewUI borderless-window example should be checked before implementation.

## Open Questions

- Should title-bar menus be shell-owned only in V1, or should plugins be allowed to contribute menu items?
- Should title-bar right-side functional icons be plugin-contributable, or reserved for shell commands?
- Should always-on-top state be persisted?
- Should the center title use the active MainArea tab title, active Activity title, or an explicit shell title service?
- How should keyboard access for File/View/Help menus work?

