# Issue 018: Instantiate Runtime Plugins from Entry Point

## What to build

Resolve the manifest `entryPoint`, instantiate it, validate it implements `IMewooPlugin`, and verify manifest id matches `plugin.Id`.

## Acceptance criteria

- [x] Missing entry point type fails the plugin only.
- [x] Entry point not implementing `IMewooPlugin` fails the plugin only.
- [x] Entry point constructor failure is logged.
- [x] Manifest id mismatch is rejected.
- [x] Valid runtime plugin instances can be handed to `MewooPluginHost`.

## Blocked by

- Issue 017

## Status

Done

