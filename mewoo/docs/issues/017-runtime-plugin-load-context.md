# Issue 017: Implement Runtime Plugin Load Context

## What to build

Implement a collectible per-plugin load context backed by `AssemblyDependencyResolver`.

## Acceptance criteria

- [x] Runtime plugin assemblies load from the manifest assembly path.
- [x] Plugin-private dependencies resolve from the plugin directory.
- [x] `Mewoo.Abstractions` resolves from the default context.
- [x] MewUI assemblies used by plugin views resolve from the default context.
- [x] Failed assembly loads are logged and do not crash startup.

## Blocked by

- Issue 016

## Status

Done

