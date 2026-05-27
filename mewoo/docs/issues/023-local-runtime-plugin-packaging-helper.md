# Issue 023: Local Runtime Plugin Packaging Helper

## What to build

Add a simple packaging command that publishes a plugin project into `%LocalAppData%\Mewoo\Plugins\<pluginId>`.

## Acceptance criteria

- [x] Packaging output includes `mewoo.plugin.json`.
- [x] Packaging output includes the plugin assembly.
- [x] Packaging output includes plugin-private dependencies.
- [x] Packaging does not copy host-shared assemblies unnecessarily.
- [x] A packaged sample runtime plugin can be discovered by Mewoo.

## Blocked by

- Issue 019

## Status

Done

