# ADR 0001: Build Mewoo as a VSCode-like Plugin Host

## Status

Accepted

## Context

Mewoo will be used as a personal desktop UI template for future small applications. The desired layout is inspired by VSCode, but the primary user value is not code editing. Mewoo should provide a consistent host shell, theme system, layout regions, command model, and extension points. Individual apps provide their own UI details and domain-specific behavior.

The first known hosted apps are a quick launcher and a trading K-line analyzer. These apps are different enough that Mewoo should avoid hard-coding app-specific navigation or editor assumptions into the shell.

## Decision

Mewoo will be designed as a host shell with app/plugin contribution points rather than as a standalone editor or a general-purpose widget library.

The first version will include:

- Fixed VSCode-like visual regions.
- Collapsible sidebar and bottom panel.
- Tabbed document area.
- Command registry.
- Theme tokens.
- Status bar contribution points.
- A plugin model considered from the start, with the full loading/runtime strategy to be decided separately.

## Consequences

Shell APIs should describe regions, commands, views, documents, themes, and contributions rather than editor-specific concepts.

The shell must leave domain UI ownership to hosted apps. For example, the quick launcher and trading analyzer should be able to contribute different views, tabs, commands, and status items without changing shell internals.

MewUI v0.15.2 is the initial implementation baseline, but API details must be verified before coding because local source may differ from the latest published release.

## Open Questions

- Are plugins compiled together with the host, loaded dynamically from assemblies, or both?
- Is a plugin a whole app/workspace, a feature module inside one app, or both?
- Which contribution points are required in the first vertical slice?
- How much VSCode behavior should be reproduced before the first real app is built?

