# ADR 0012: Use Quick Launcher as the First Tracer Bullet

## Status

Accepted

## Context

The first real Mewoo plugin should validate the shell architecture without overwhelming the initial implementation with domain complexity. A quick launcher is simple enough to build early, but still exercises the plugin model, ActivityBar, Sidebar, MainArea, command registry, StatusBar, storage, and theme behavior.

## Decision

The first tracer bullet is `Mewoo.Plugins.QuickLauncher`.

Initial UI:

```text
ActivityBar
  Launcher icon

Sidebar
  Search input
  Groups list
  Recent list

MainArea tab: Launcher
  Search box
  List of launcher items
  Item detail/actions

StatusBar
  Item count / selected item / last run result
```

V1 uses JSON configuration.

Example shape:

```json
{
  "groups": [
    {
      "id": "dev",
      "title": "Development",
      "items": [
        {
          "id": "github",
          "title": "GitHub",
          "kind": "url",
          "target": "https://github.com",
          "tags": ["code"]
        }
      ]
    }
  ]
}
```

Launcher item kinds:

- URL.
- File.
- Executable.
- Script.

Initial commands:

- `quickLauncher.open`
- `quickLauncher.focusSearch`
- `quickLauncher.runSelected`
- `quickLauncher.reload`
- `quickLauncher.openConfig`

V1 uses a list UI first. Grid view can come later.

V1 does not include:

- Icon downloading.
- Complex script permission model.
- Drag-and-drop sorting.
- Cloud sync.
- Advanced fuzzy search.

Script items are supported as local targets, but V1 does not design a broad security or sandboxing model for scripts.

## Consequences

The Quick Launcher validates Mewoo's plugin and workbench systems with a useful personal workflow.

The first implementation can stay focused on architecture while still producing a usable plugin.

The trading K-line analyzer remains a future plugin and should not drive the first shell implementation.

