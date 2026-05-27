# Issue 024: Create an External Runtime Plugin Smoke Sample

## What to build

Create a small external runtime plugin sample that is packaged into `%LocalAppData%\Mewoo\Plugins` and loaded by Mewoo without being registered directly by `Mewoo.App`.

## Acceptance criteria

- [x] Sample plugin is not registered as a compiled plugin by `Mewoo.App`.
- [x] Sample plugin package includes `mewoo.plugin.json`.
- [x] Sample plugin contributes an ActivityBar item.
- [x] Sample plugin contributes a Sidebar view.
- [x] Sample plugin contributes a MainArea view.
- [x] Sample plugin contributes a command.
- [x] Sample plugin contributes a StatusBar item.
- [x] Mewoo startup loads and activates the packaged sample plugin from the runtime plugin directory.

## Blocked by

- Slice 023

## Status

Done

## Verification

- `dotnet build Mewoo.slnx`
- `dotnet run --project tools/Mewoo.PluginPackager/Mewoo.PluginPackager.csproj -- --project samples/Mewoo.Samples.RuntimeSmokePlugin/Mewoo.Samples.RuntimeSmokePlugin.csproj --id mewoo.samples.runtimeSmoke --display-name "Runtime Smoke Plugin" --version 0.1.0 --entry-point Mewoo.Samples.RuntimeSmokePlugin.RuntimeSmokePlugin --configuration Debug --minimum-mewoo-version 1.0.0`
- `dotnet run --project src/Mewoo.App/Mewoo.App.csproj`
