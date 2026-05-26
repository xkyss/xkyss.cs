# ADR 0010: Define V1 Command, State, and Theme Systems

## Status

Accepted

## Context

Mewoo needs command execution, shell state persistence, plugin state storage, and theme tokens before the Workbench and Quick Launcher tracer bullet can be implemented. These systems should be simple enough for V1 while leaving room for richer keybindings, menu integration, and plugin behavior later.

## Decision

### Command System

Commands are globally namespaced and owned by plugins or the shell.

Conceptual descriptor:

```csharp
public sealed record MewooCommandDescriptor(
    string Id,
    string OwnerPluginId,
    string Title,
    string? Category,
    string? Icon,
    string? DefaultKeyBinding,
    Func<IMewooCommandContext, CancellationToken, ValueTask> ExecuteAsync,
    Func<IMewooCommandContext, bool>? CanExecute = null
);
```

V1 command capabilities:

- Execute by ID.
- `CanExecute`.
- Command palette data source.
- Optional default keybinding metadata.
- Future reuse from menus, title-bar actions, and StatusBar items.
- Command revocation when a plugin unloads.

Duplicate command IDs are errors. Default keybinding conflicts are errors in V1; later versions may introduce user-facing conflict resolution.

V1 may store keybinding metadata without implementing a full keybinding resolver. If key execution is implemented, it should be minimal and global.

### State System

V1 uses shell-owned mutable state with change events rather than a heavy reactive framework.

Core shell state includes:

- Active Activity ID.
- Sidebar collapsed state.
- Sidebar width.
- Panel visibility.
- Panel height.
- Active MainArea view ID.
- Open MainArea views.
- Theme ID.
- Always-on-top state.

Shell state and plugin state are separate.

Shell state is saved and restored by Mewoo. Plugin business state is saved by plugins through `IPluginStorage`.

State persistence uses JSON behind storage abstractions such as `IStateStorage` and `IPluginStorage`. Concrete storage paths are decided by `Mewoo.App`.

### Theme System

Themes use strongly typed base tokens plus an extension dictionary.

Conceptual theme:

```csharp
public sealed class MewooTheme
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required MewooThemeTokens Tokens { get; init; }
    public Dictionary<string, object> Extensions { get; } = new();
}
```

Token groups include:

- Core tokens: window, title bar, activity bar, sidebar, main area, panel, status bar, text, border, accent, font, and spacing.
- Component tokens: active ActivityBar item, tabs, StatusBar hover state, and other component-specific values.

V1 includes built-in Dark and Light themes. High Contrast is reserved for later.

Plugins may override theme tokens but may not register full themes in V1.

## Consequences

The command system is simple, deterministic, and safe across multiple plugins.

The state model can be implemented and tested without UI rendering.

JSON storage keeps V1 practical while allowing storage location and format details to evolve behind abstractions.

Theme tokens give the Workbench a consistent styling contract while letting plugins make constrained visual adjustments.

