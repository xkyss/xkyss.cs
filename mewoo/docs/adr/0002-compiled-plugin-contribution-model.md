# ADR 0002: Use Compiled Plugins and Explicit Contribution Points

## Status

Accepted

## Context

Mewoo should let future personal applications run inside the same VSCode-like shell. The first real app will be a quick launcher, followed by a trading K-line analyzer. Plugin design must be considered from the start, but the first version should avoid the complexity of dynamic loading, unloading, version isolation, and hot reload.

The shell is not an editor, so the central content region should avoid editor-specific naming.

## Decision

The first version will use compiled plugins. A plugin is a C# module referenced by the host and registered at startup.

Plugins may contribute:

- Activity Bar items.
- Sidebar views.
- MainArea tabs/views.
- Bottom panel views.
- Commands.
- Status bar items.
- Optional theme token overrides.

Multiple plugins may run at the same time. A single plugin may contribute one or more Activity Bar items.

The central tabbed content region will be called `MainArea`.

The first tracer bullet will be a Quick Launcher plugin that validates the shell, contribution model, command registry, status bar contribution, and a real MainArea view.

## Consequences

The first plugin API can be simple and strongly typed. It does not need assembly scanning, plugin manifest files, plugin unloading, or runtime isolation yet.

The shell should own layout, selection, focus, command dispatch, theme application, and contribution placement. Plugins should own their domain UI and commands, but should not directly mutate global shell layout internals.

Because multiple plugins can contribute Activity Bar items, the shell needs a stable identity model for activities, views, commands, and tabs.

## Open Questions

- Should plugin registration be imperative, declarative, or a mix of both?
- Should Activity Bar items be grouped by plugin, or ordered globally?
- Should MainArea tabs be persisted across restarts in the first version?
- What lifecycle hooks does a compiled plugin need beyond registration?

