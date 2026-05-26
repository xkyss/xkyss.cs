# ADR 0004: Use a Phased Plugin Runtime Strategy

## Status

Accepted

## Context

Mewoo needs a full plugin lifecycle, but the first version does not need the full complexity of runtime plugin discovery and isolation. The immediate goal is to validate the VSCode-like shell, contribution model, MainArea, command registry, theme tokens, status bar contributions, and the Quick Launcher tracer bullet.

Runtime plugin systems introduce additional concerns: assembly scanning, dependency conflicts, plugin manifests, unload reliability, version compatibility, permissions, update flow, and stronger error isolation. These concerns are important, but they are not required to validate the first product slice.

## Decision

Mewoo will use a phased plugin strategy.

### V1: Compiled Plugins with Runtime Lifecycle

Plugins are C# modules referenced by the host and registered at startup.

V1 still designs lifecycle and contribution ownership as if plugins may later become runtime-loaded:

- Register.
- Activate.
- Deactivate.
- Unload contributions.
- Dispose.
- Failure isolation.
- Contribution revocation.

No runtime assembly scanning, plugin directory, manifest loading, hot reload, dependency isolation, or marketplace/update behavior is required in V1.

### V2: Manifest and Runtime Assembly Loading

V2 may introduce runtime-loaded plugin assemblies.

Expected additions:

- Plugin directory discovery.
- Plugin manifest, for example `plugin.json`.
- Plugin entry type declaration.
- Plugin version.
- Compatible Mewoo version range.
- Basic dependency metadata.
- Runtime enable/disable from discovered plugins.

### V3: Isolation and Marketplace-like Model

V3 may introduce stronger isolation and distribution capabilities.

Expected additions:

- Plugin dependency resolution.
- Assembly load isolation.
- More reliable unload behavior.
- Permission or capability declarations.
- Update/install flow.
- Stronger crash and activation failure recovery.
- Potential script-based or external-process plugins if needed.

## Consequences

V1 can move quickly without blocking on plugin infrastructure that is not yet needed.

The lifecycle API should still be explicit enough to avoid rewriting plugin ownership when V2 runtime loading arrives.

The shell should own contribution registration and revocation from the start. This keeps V1 compatible with future dynamic plugin unloading.

V2 and V3 are roadmap directions, not commitments to implement before the first usable Mewoo shell.

