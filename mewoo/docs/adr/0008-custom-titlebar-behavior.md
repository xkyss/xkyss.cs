# ADR 0008: Keep the V1 Custom Title Bar Shell-Owned

## Status

Accepted

## Context

Mewoo uses a borderless window with a custom title bar. The title bar contains app identity, menus, active title, shell actions, always-on-top, theme switching, and window controls. Because the title bar owns window behavior, it needs stricter boundaries than ordinary plugin UI.

## Decision

In V1, title-bar menus are shell-owned. The shell provides menus such as File, View, and Help. Plugin menu contribution APIs may be reserved for the future, but plugins do not directly populate the title bar in V1.

In V1, right-side title-bar action icons are shell-owned. Expected actions include theme switching, always-on-top, and layout or panel toggles. Plugin status should use StatusBar contributions instead of title-bar actions.

The center title is provided by a workbench title service. Title priority:

```text
Active MainArea tab title
-> active Activity title
-> app name
```

Always-on-top state is persisted as a shell preference.

The custom borderless window must support:

- Dragging the title bar to move the window.
- Double-clicking the title bar to maximize or restore.
- Minimize, maximize/restore, and close buttons.
- Correct maximize behavior around screen work area.
- Resizable window edges.
- Always-on-top toggle.
- Window-state-aware maximize/restore icon.

TitleBar height is fixed at `34px`.

## Consequences

The title bar stays predictable in V1 and cannot be destabilized by plugin UI.

Menu and title action contribution points can be introduced later without changing the core borderless-window behavior.

The workbench needs a title service and persisted shell preferences before the title bar can be fully wired.

MewUI's borderless-window examples should be reviewed before implementation.

