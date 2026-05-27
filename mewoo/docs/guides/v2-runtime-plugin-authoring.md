# Mewoo V2 Runtime Plugin Authoring Guide

This guide describes the V2 trusted-local runtime plugin workflow: create a plugin assembly, package it into the local plugin root, diagnose it in Mewoo, and remove or unload it when needed.

## Project Shape

A runtime plugin is a normal `.NET 10` class library that implements `IMewooPlugin`.

During in-repo development, reference:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\Mewoo.Abstractions\Mewoo.Abstractions.csproj" />
</ItemGroup>

<ItemGroup>
  <PackageReference Include="Aprillz.MewUI" Version="0.15.2" />
</ItemGroup>
```

Rules:

- Reference `Mewoo.Abstractions` for plugin contracts, contribution descriptors, commands, and lifecycle interfaces.
- Reference `Aprillz.MewUI` when the plugin contributes MewUI views.
- Do not reference `Mewoo.App`, `Mewoo.Workbench`, or `Mewoo.Core` from plugin projects.
- Target `net10.0`.

The smoke sample is the current reference implementation:

```text
samples/Mewoo.Samples.RuntimeSmokePlugin
```

## Plugin Entry Point

The manifest `entryPoint` must name a public type that implements `IMewooPlugin`.

```csharp
public sealed class RuntimeSmokePlugin : IMewooPlugin
{
    public string Id => "mewoo.samples.runtimeSmoke";

    public string DisplayName => "Runtime Smoke Plugin";

    public void Register(IMewooContributionRegistry registry)
    {
        registry.Activity("mewoo.samples.runtimeSmoke.activity")
            .Title("Runtime")
            .Icon("R")
            .ViewContainer("mewoo.samples.runtimeSmoke.container");
    }
}
```

The plugin `Id` must match the manifest `id`. If they differ, loading fails and the failure is shown in Runtime Diagnostics and Logs.

## Manifest

Every runtime plugin directory must contain:

```text
mewoo.plugin.json
```

Example:

```json
{
  "id": "mewoo.samples.runtimeSmoke",
  "displayName": "Runtime Smoke Plugin",
  "version": "0.1.0",
  "assembly": "Mewoo.Samples.RuntimeSmokePlugin.dll",
  "entryPoint": "Mewoo.Samples.RuntimeSmokePlugin.RuntimeSmokePlugin",
  "minimumMewooVersion": "1.0.0",
  "disabled": false,
  "metadata": {
    "author": "xkyss"
  }
}
```

Required fields:

- `id`: stable plugin id. Prefer a namespaced id.
- `displayName`: user-facing plugin name.
- `version`: package version.
- `assembly`: relative path to the plugin assembly inside the plugin directory.
- `entryPoint`: fully-qualified entry type implementing `IMewooPlugin`.

Optional fields:

- `minimumMewooVersion`: blocks loading when the host is older.
- `disabled`: skips loading when `true`.
- `metadata`: string key-value diagnostics metadata.

`assembly` must be relative and must stay inside the plugin directory.

## Package

Use the local packager:

```powershell
dotnet run --project tools/Mewoo.PluginPackager/Mewoo.PluginPackager.csproj -- `
  --project samples/Mewoo.Samples.RuntimeSmokePlugin/Mewoo.Samples.RuntimeSmokePlugin.csproj `
  --id mewoo.samples.runtimeSmoke `
  --display-name "Runtime Smoke Plugin" `
  --version 0.1.0 `
  --entry-point Mewoo.Samples.RuntimeSmokePlugin.RuntimeSmokePlugin `
  --configuration Debug `
  --minimum-mewoo-version 1.0.0
```

By default, the packager publishes to:

```text
%LocalAppData%\Mewoo\Plugins\<plugin-id>
```

Pass `--output-root <dir>` to package into a different plugin root.

The packager:

- runs `dotnet publish`;
- writes `mewoo.plugin.json`;
- removes host-shared assemblies from the package.

## Install Layout

Mewoo scans only immediate child directories under:

```text
%LocalAppData%\Mewoo\Plugins
```

Expected layout:

```text
%LocalAppData%\Mewoo\Plugins
└─ mewoo.samples.runtimeSmoke
   ├─ mewoo.plugin.json
   ├─ Mewoo.Samples.RuntimeSmokePlugin.dll
   └─ plugin-private-dependency.dll
```

Nested plugin directories are ignored.

## Host-Shared Assemblies

Runtime plugins load in per-plugin collectible load contexts, but these assemblies are shared with the host:

- `Mewoo.Abstractions`
- `Aprillz.MewUI`
- .NET framework assemblies

Do not ship private copies of host-shared assemblies in the plugin package. The packager removes the known shared assemblies automatically.

Plugin-private dependencies should be published beside the plugin assembly. They can vary per plugin as long as their types do not cross the Mewoo plugin contract boundary.

## Diagnose

Open the Runtime Diagnostics activity in Mewoo to inspect discovered runtime plugins.

Use it to:

- see plugin id, version, state, manifest path, and messages;
- reload a discovered plugin;
- unload a loaded plugin;
- enable or disable a plugin by updating `disabled` in the manifest.

Use the Logs panel for detailed loader and lifecycle messages. Typical failures include invalid JSON, missing assemblies, missing entry point types, id mismatch, incompatible `minimumMewooVersion`, registration exceptions, activation exceptions, and unload verification risks.

## Remove or Unload

Unload through Runtime Diagnostics when Mewoo is running.

To remove a plugin permanently:

1. Unload it in Runtime Diagnostics.
2. Delete its directory under `%LocalAppData%\Mewoo\Plugins`.
3. Restart Mewoo if the plugin had active UI, long-running tasks, or native resources.

Unload is best-effort in V2. Mewoo requests load-context unload and logs whether the context was collected, but plugin code must release its own references during `DeactivateAsync`, `UnloadAsync`, or `DisposeAsync`.

Common unload blockers:

- static references to plugin objects;
- background tasks or timers still running;
- event handlers left subscribed on host or global objects;
- plugin-owned windows, streams, processes, or native handles;
- plugin types cached outside the plugin load context.

V2 runtime plugins are trusted local code running in the Mewoo process. V2 does not provide a sandbox, package signatures, permission prompts, or out-of-process isolation.
