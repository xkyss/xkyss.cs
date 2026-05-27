# ADR 0013: Use Trusted Local Runtime Plugins with Per-Plugin Load Contexts

## Status

Accepted

## Context

V1 uses compiled plugins referenced by `Mewoo.App`. This validated the workbench shell, contribution model, command registry, theme switching, state persistence, logs, and Quick Launcher tracer bullet.

V2 needs runtime plugin loading so personal apps can be added without rebuilding the shell. The existing V1 lifecycle and contribution ownership model should remain valid.

Runtime loading introduces trade-offs:

- Loading every plugin into the default context is simple, but dependency conflicts and unload become painful.
- Loading every plugin into a separate process gives stronger isolation, but it is too much infrastructure for the next personal-use milestone.
- Loading trusted plugin assemblies into collectible load contexts keeps the app simple while preparing for dependency separation and best-effort unload.

## Decision

Mewoo V2 will treat runtime plugins as trusted local code loaded into the Mewoo process.

Each runtime plugin is discovered from a local plugin directory containing `mewoo.plugin.json`.

Each runtime plugin should load through a per-plugin collectible load context.

Host-shared assemblies must resolve from the default context:

- `Mewoo.Abstractions`
- MewUI assemblies used by plugin views
- .NET framework assemblies

Plugin-private assemblies resolve from the plugin directory.

The runtime loader creates an `IMewooPlugin` instance from the manifest `entryPoint`, validates it, and then hands it to the existing `MewooPluginHost`. The existing host remains responsible for registration, activation, deactivation, unload, dispose, failure handling, and contribution revocation.

V2 does not implement a security sandbox, marketplace, signing, automatic dependency download, or hot reload.

## Consequences

V2 can add runtime plugins without rewriting the V1 plugin model.

Plugin-specific dependency conflicts become more manageable than default-context loading.

Unload can be attempted after lifecycle cleanup, but it remains best-effort because live references from UI, events, tasks, or static state can keep a load context alive.

Plugin authors must understand that runtime plugins are trusted local code running in-process.

Future V3 work can add signatures, permissions, package management, or out-of-process plugins without changing the meaning of V2 manifests.

## Rejected Alternatives

### Default-context runtime loading

This was rejected because it makes dependency conflicts more likely and makes unload effectively impossible.

### Out-of-process plugins in V2

This was rejected because Mewoo first needs a smooth personal plugin workflow. Out-of-process UI hosting, IPC, lifecycle bridging, and command routing would slow down the second phase too much.

### Public marketplace model in V2

This was rejected because the current product is a personal desktop shell. Marketplace concerns belong in V3 after local runtime loading is proven.

