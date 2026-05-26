# ADR 0003: Use Stable Workbench Contribution Identities

## Status

Accepted

## Context

Mewoo supports multiple compiled plugins running at the same time. A single plugin can contribute one or more Activity Bar items. The shell therefore needs stable identities for activities, view containers, sidebar views, MainArea views, commands, and persisted layout state.

The first implementation should stay simple, but the model should not block later VSCode-like behavior.

## Decision

The selected Activity Bar item is represented by `ActivityId`, not by `PluginId`.

Activity Bar items point to view containers:

```text
ActivityBarItem -> ViewContainer -> SidebarView[]
```

The first version may render one sidebar view per container, but the API will allow multiple views so future plugins can expose richer navigation without changing the shell model.

Plugins do not directly mutate the MainArea tab collection. They open content through a shell-owned service, conceptually:

```csharp
IWorkbenchService.OpenMainView(...)
```

The shell owns tab activation, deduplication, close behavior, and future restore behavior.

Command IDs are globally namespaced strings. Examples:

- `quickLauncher.open`
- `quickLauncher.runSelected`
- `kline.openSymbol`
- `workbench.toggleSidebar`

The first version will include a full plugin lifecycle requirement. The exact lifecycle states, hooks, error behavior, unload semantics, and persistence boundaries will be decided separately.

Basic state persistence boundaries:

- Shell owns active activity, opened MainArea tab identities, sidebar collapsed state, and panel collapsed state.
- Plugins own their own business state.

## Consequences

The shell can host multiple plugins without conflating plugin identity with UI selection.

The Activity Bar and Sidebar can evolve toward VSCode-like view containers without reworking the first API.

The MainArea remains shell-controlled, which keeps layout, tab lifecycle, and restore behavior consistent across plugins.

Global command IDs reduce ambiguity when multiple plugins are active.

## Open Questions

- What are the exact plugin lifecycle states?
- Can a plugin be unloaded or disabled at runtime in the first version?
- How does the shell handle plugin activation failure?
- Do lifecycle hooks run sequentially or can independent plugins initialize in parallel?
- What cancellation and disposal contracts are required?

