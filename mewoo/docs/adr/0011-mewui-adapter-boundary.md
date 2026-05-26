# ADR 0011: Isolate MewUI Details in the Workbench Adapter Layer

## Status

Accepted

## Context

Mewoo is built on top of MewUI, with v0.15.2 as the intended baseline. The local MewUI source may be newer than v0.15.2, so Mewoo should avoid leaking MewUI-specific API assumptions throughout the architecture.

Plugins need to provide UI, but core plugin contracts should remain stable and should not force all abstractions to reference MewUI directly.

## Decision

`Mewoo.Abstractions` does not reference MewUI.

Plugins may reference MewUI to build their own views, but they return views through `IMewooView`.

Conceptual view contract:

```csharp
public interface IMewooView
{
    string Id { get; }
    object NativeView { get; }
}
```

`Mewoo.Workbench` owns the MewUI adapter layer:

```text
Mewoo.Workbench
  MewuiViewHost
  MewuiThemeApplier
  MewuiBorderlessWindow
  MewuiWorkbenchShell
```

Workbench responsibilities:

- Check that `IMewooView.NativeView` is a MewUI-compatible control.
- Show an error view if a plugin returns an unsupported native view.
- Map Mewoo theme tokens to MewUI styling.
- Isolate MewUI v0.15.2/local API differences.
- Encapsulate borderless-window behavior in a dedicated module based on MewUI examples.

## Consequences

`Mewoo.Abstractions` and `Mewoo.Core` can be implemented and tested without MewUI references.

MewUI API changes are contained inside `Mewoo.Workbench`.

Plugins keep the flexibility to build native MewUI UI while the shell keeps control over hosting, theme application, and error handling.

