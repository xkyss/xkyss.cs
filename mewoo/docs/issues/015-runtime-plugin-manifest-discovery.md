# Issue 015: Add Runtime Plugin Manifest Discovery

## What to build

Define the V2 runtime plugin manifest contract and a catalog that scans local plugin directories for `mewoo.plugin.json`. Do not load external assemblies yet.

## Acceptance criteria

- [x] A runtime plugin manifest model exists.
- [x] Manifest fields include plugin id, display name, version, assembly path, and entry point type.
- [x] Manifest assembly paths must be relative and stay inside the plugin directory.
- [x] A runtime plugin catalog scans the local plugin root for manifest files.
- [x] Invalid manifests are logged without crashing app startup.
- [x] App startup discovers runtime plugin manifests without affecting compiled V1 plugins.

## Blocked by

- V1 baseline

## Status

Done

