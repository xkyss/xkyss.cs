# ADR 0009: Use Fluent Plugin Contribution Registration Backed by Descriptors

## Status

Accepted

## Context

Mewoo plugins need to contribute Activity Bar items, Sidebar views, MainArea views, commands, StatusBar items, and theme token overrides. Plugin authors should have a friendly registration API, while the shell needs structured data for validation, ownership, lifecycle, and future runtime loading.

## Decision

Plugins register contributions through a fluent builder API. Internally, the registry stores descriptor objects.

Conceptual plugin registration:

```csharp
public void Register(IMewooContributionRegistry registry)
{
    registry.Activity("quickLauncher.activity")
        .Title("Launcher")
        .Icon("rocket")
        .ViewContainer("quickLauncher.views");

    registry.ViewContainer("quickLauncher.views")
        .Title("Launcher")
        .AddView("quickLauncher.shortcuts", view => view
            .Title("Shortcuts")
            .Create(ctx => new QuickLauncherSidebarView(ctx)));

    registry.MainView("quickLauncher.home")
        .Title("Launcher")
        .CanOpenMultiple(false)
        .Create(ctx => new QuickLauncherHomeView(ctx));

    registry.Command("quickLauncher.open")
        .Title("Open Launcher")
        .Execute(async (ctx, cancellationToken) =>
        {
            await ctx.Workbench.OpenMainViewAsync("quickLauncher.home", cancellationToken);
        });

    registry.StatusBarItem("quickLauncher.status")
        .AlignLeft()
        .Text("Launcher ready");
}
```

Internal descriptor types include:

- `ActivityDescriptor`
- `ViewContainerDescriptor`
- `SidebarViewDescriptor`
- `MainViewDescriptor`
- `CommandDescriptor`
- `StatusBarItemDescriptor`
- `ThemeTokenOverrideDescriptor`

Rules:

- Contribution IDs are strings.
- IDs must use a global namespace format such as `quickLauncher.open`.
- Core validates duplicate IDs and ID format.
- The registry automatically assigns the current `PluginId` as contribution owner.
- View creation is lazy. `Register` only declares contributions and must not create UI controls.
- View factories return `IMewooView`.
- Commands are asynchronous and return `ValueTask`.
- StatusBar contributions can be updated at runtime through shell-provided handles.
- Plugins may override theme tokens but may not register full themes in V1.

Conceptual view contract:

```csharp
public interface IMewooView
{
    string Id { get; }
    object NativeView { get; }
}
```

## Consequences

The fluent API keeps plugin code readable, while descriptors keep the shell implementation structured.

Lazy view creation keeps registration side-effect-light and makes plugin activation, unloading, and failure isolation easier.

Returning `IMewooView` keeps `Mewoo.Abstractions` from depending directly on MewUI-specific control types, while still allowing the Workbench to adapt native views.

Async commands provide one execution model for both fast and slow actions.

StatusBar handles allow dynamic plugin status without making the descriptor layer mutable UI state.

