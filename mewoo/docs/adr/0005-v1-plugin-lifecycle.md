# ADR 0005: Define the V1 Plugin Lifecycle

## Status

Accepted

## Context

Mewoo V1 uses compiled plugins, but the shell still needs runtime lifecycle management so plugins can be activated, deactivated, unloaded, and disposed in a controlled way. The lifecycle must support future runtime-loaded plugins without forcing V1 to implement manifest discovery or assembly isolation.

## Decision

V1 plugin lifecycle states:

```text
Created
  -> Registered
  -> Activated
  -> Deactivated
  -> Unloaded
  -> Disposed

Any state -> Failed
```

Registration and runtime lifecycle are separate concepts.

Conceptual interfaces:

```csharp
public interface IMewooPlugin
{
    string Id { get; }
    string DisplayName { get; }

    void Register(IMewooContributionRegistry registry);
}

public interface IMewooPluginLifecycle
{
    ValueTask ActivateAsync(IMewooPluginContext context, CancellationToken cancellationToken);
    ValueTask DeactivateAsync(CancellationToken cancellationToken);
    ValueTask UnloadAsync(CancellationToken cancellationToken);
    ValueTask DisposeAsync();
}
```

Lifecycle decisions:

- Deactivated plugin contributions are hidden by default.
- Unloading a plugin closes MainArea tabs owned by that plugin.
- Close guards may be added for plugin-owned MainArea tabs with unsaved or transient state.
- Plugin failures must not crash the shell.
- `Register` failure makes the plugin invisible.
- `ActivateAsync` failure hides the plugin's contributions and records the error.
- `DeactivateAsync`, `UnloadAsync`, or `DisposeAsync` failures are recorded, but shell cleanup continues.
- Plugins may run background tasks only through shell-managed context and cancellation.
- V1 does not implement plugin dependency resolution.
- V1 defines plugin-scoped storage through an `IPluginStorage` abstraction. The first implementation may be simple.

## Consequences

The shell owns lifecycle orchestration, contribution visibility, cancellation, and cleanup.

Plugins can be disabled or unloaded without leaving orphaned Activity Bar items, Sidebar views, MainArea tabs, commands, or status bar items.

Separating registration from lifecycle keeps static contribution declaration distinct from runtime behavior.

Future runtime loading can reuse the same lifecycle contract.

## Open Questions

- Should close guards be implemented in V1 or only reserved in the API?
- Should plugin storage be JSON-backed in V1, or should V1 start with an interface and in-memory implementation?
- How should lifecycle errors be surfaced in the UI: status bar, notification, log panel, or all of them?

