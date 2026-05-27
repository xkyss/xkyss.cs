# Mewoo V2 Runtime Plugin Design

## Purpose

V2 turns Mewoo from a compiled-plugin shell into a local runtime-plugin shell.

The goal is not a public marketplace or an untrusted sandbox. Mewoo is a personal desktop host, so V2 optimizes for:

- Installing or removing local plugins without rebuilding `Mewoo.App`.
- Keeping the V1 lifecycle and contribution model intact.
- Making plugin failures visible and reversible.
- Preparing for stronger V3 isolation later.

## Non-goals

V2 does not provide a security sandbox. Runtime plugins are trusted local code and run in the Mewoo process.

V2 does not implement marketplace install/update, signed packages, automatic dependency download, external process plugins, or cross-user plugin management.

V2 does not guarantee perfect unload for every plugin. Unload is best-effort and requires plugin code to release references during lifecycle cleanup.

## Runtime Plugin Directory

The local runtime plugin root is:

```text
%LocalAppData%\Mewoo\Plugins
```

Each plugin owns one child directory:

```text
%LocalAppData%\Mewoo\Plugins
└─ quickLauncherPlus
   ├─ mewoo.plugin.json
   ├─ QuickLauncherPlus.dll
   └─ dependency.dll
```

Mewoo scans only immediate child directories of the plugin root. Nested plugin directories are ignored.

## Manifest

Manifest file name:

```text
mewoo.plugin.json
```

Required fields:

- `id`: stable plugin id. Must be globally namespaced, for example `quickLauncherPlus` or `xkyss.quickLauncherPlus`.
- `displayName`: user-facing plugin name.
- `version`: plugin package version.
- `assembly`: relative path from the plugin directory to the plugin assembly.
- `entryPoint`: fully-qualified type name implementing `IMewooPlugin`.

Optional fields:

- `minimumMewooVersion`: lowest compatible Mewoo version.
- `disabled`: local skip flag.
- `metadata`: string key-value metadata for diagnostics and future UI.

Example:

```json
{
  "id": "xkyss.quickLauncherPlus",
  "displayName": "Quick Launcher Plus",
  "version": "0.1.0",
  "assembly": "QuickLauncherPlus.dll",
  "entryPoint": "Xkyss.Mewoo.Plugins.QuickLauncherPlus.QuickLauncherPlusPlugin",
  "minimumMewooVersion": "1.0.0",
  "disabled": false,
  "metadata": {
    "author": "xkyss"
  }
}
```

Validation rules:

- `assembly` must be relative.
- `assembly` must stay inside the plugin directory.
- Missing required fields fail discovery for that plugin only.
- Invalid manifests are logged and do not crash app startup.
- Disabled plugins are skipped and logged.

## Loading Model

V2 introduces a runtime plugin loader after manifest discovery.

Recommended loading flow:

```text
Discover manifest
  -> Validate manifest
  -> Create plugin load context
  -> Load plugin assembly
  -> Resolve entryPoint type
  -> Instantiate IMewooPlugin
  -> Validate manifest id == plugin.Id
  -> Register with MewooPluginHost
  -> Activate through existing lifecycle
```

The existing `MewooPluginHost` remains the lifecycle owner. Runtime loading creates plugin instances; it does not replace registration, activation, deactivation, unload, or contribution revocation.

## Assembly Boundaries

Each runtime plugin should load in its own collectible load context.

Host-shared assemblies must resolve from the default context so type identity stays stable:

- `Mewoo.Abstractions`
- MewUI assemblies used by plugin views
- .NET framework assemblies

Plugin-private dependencies resolve from the plugin directory.

This keeps `IMewooPlugin`, contribution descriptors, and MewUI control types compatible with the host while still allowing most plugin-specific dependencies to live beside the plugin.

## Lifecycle Integration

Runtime plugins use the same lifecycle states as V1:

```text
Created -> Registered -> Activated -> Deactivated -> Unloaded -> Disposed
Any state -> Failed
```

Additional V2 host-owned states may be tracked around the existing lifecycle:

```text
Discovered -> Loaded -> Registered
LoadFailed
```

These states belong to runtime plugin infrastructure, not to `IMewooPlugin`.

Unload sequence:

```text
Deactivate plugin
  -> Remove visible contributions
  -> Close plugin-owned MainArea tabs
  -> Call plugin unload/dispose lifecycle hooks
  -> Release loader references
  -> Request load-context unload
  -> Log success or remaining unload risk
```

## Dependency Strategy

V2 uses local-copy dependencies. A plugin package must include the non-shared assemblies it needs.

Mewoo does not download dependencies, merge plugin dependency graphs, or resolve NuGet packages at runtime in V2.

If two plugins need different versions of the same private dependency, separate load contexts should allow them to coexist as long as the dependency does not cross the host/plugin contract boundary.

## Compatibility

V2 starts with `minimumMewooVersion`.

Compatibility checks are conservative:

- If `minimumMewooVersion` is greater than the running Mewoo version, discovery succeeds but loading is blocked.
- Missing `minimumMewooVersion` means "compatible with the current local build" for personal use.

A future `maximumMewooVersion` or version range can be added if real compatibility failures appear.

## Failure Handling

Failure handling follows the V1 rule: plugin failures must not crash the shell.

Failure surfaces:

- Log panel entry.
- Plugin catalog state for future UI.
- Sidebar or Activity-level unavailable state after a failed registration/activation.

Failure examples:

- Manifest cannot be parsed.
- Manifest entry point type is missing.
- Assembly cannot be loaded.
- Entry point type does not implement `IMewooPlugin`.
- Manifest id and plugin id differ.
- Registration throws.
- Activation throws.
- Unload cannot fully release the load context.

## Enable, Disable, and Reload

V2 supports startup discovery and local disable through `disabled: true`.

Manual reload can be added later, but it must perform the full unload sequence before reloading a plugin from disk.

Hot reload while a plugin is active is out of scope for V2.

## Security and Trust

Runtime plugins are trusted local code.

V2 does not attempt to prevent file access, process launch, network access, reflection, or native calls.

The UX should make this model clear: installing a runtime plugin is equivalent to running local code in the Mewoo process.

V3 may introduce stronger package trust, signatures, permission declarations, or out-of-process plugins.

## Implementation Slices

The recommended V2 build order is:

1. Manifest discovery.
2. Runtime plugin load context and assembly loading.
3. Runtime plugin registration with existing host lifecycle.
4. Runtime plugin unload and load-context release.
5. Runtime plugin diagnostics UI.
6. Compatibility checks and disabled-state management.
7. Packaging helper for local plugin folders.

