# Issue 044: Author Workflow and V4 Road Test

## What to build

Align the plugin author workflow with Plugin Manager and run an end-to-end V4 local package road test.

## Acceptance criteria

- [x] Packager and sample manifest documentation include publisher, trust, and permissions.
- [x] Authoring guide describes Plugin Manager install and update flows.
- [x] Road test covers package, install from file, details, disable, enable, update from file, uninstall, and broken install cleanup.
- [x] Road test records manual checks and automated verification.
- [x] Any gaps are documented before V4 baseline.

## Status

Done.

## Road Test Notes

- Added [V4 Local Plugin Authoring Guide](../guides/v4-local-plugin-authoring.md) with Plugin Manager install/update workflows and manifest metadata guidance.
- Extended `tools/Mewoo.PluginPackager` to write publisher metadata, trust declarations, permission declarations, and an optional `.mewoo-plugin` package file.
- Ran the packager against `samples/Mewoo.Samples.RuntimeSmokePlugin` and wrote `.build/v4-roadtest/runtime-smoke-0.1.0.mewoo-plugin`.
- Verified the generated package contains `mewoo.plugin.json` and `Mewoo.Samples.RuntimeSmokePlugin.dll`.
- Verified the generated manifest contains `metadata.publisher`, `metadata.publisherDisplayName`, `trust.trustedLocalCode`, and `permissions`.
- Added `MewooV4PluginManagerRoadTestTests` to automate the core road test for install, details metadata, disable, enable, update, uninstall, and broken install cleanup.
- Manual checks in this road test inspected the generated package archive and manifest metadata. A full desktop file-picker pass was not rerun in Issue 044; the automated road test covers the same core install, update, uninstall, and cleanup state transitions.
- Gap before V4 baseline: run a short final desktop smoke pass if preparing a user-facing binary. No code blockers were found.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~MewooV4PluginManagerRoadTestTests"`
- `dotnet run --no-restore --project tools\Mewoo.PluginPackager\Mewoo.PluginPackager.csproj -- --project samples\Mewoo.Samples.RuntimeSmokePlugin\Mewoo.Samples.RuntimeSmokePlugin.csproj --id mewoo.samples.runtimeSmoke --display-name "Runtime Smoke Plugin" --version 0.1.0 --entry-point Mewoo.Samples.RuntimeSmokePlugin.RuntimeSmokePlugin --configuration Debug --minimum-mewoo-version 1.0.0 --publisher xkyss --publisher-display-name "xkyss labs" --trusted-local-code true --trust-reason "Built from the in-repo sample project." --permissions filesystem --permission-reason "Reads local sample data during author testing." --output-root .build\v4-roadtest\published --package-file .build\v4-roadtest\runtime-smoke-0.1.0.mewoo-plugin`
- `tar -tf .build\v4-roadtest\runtime-smoke-0.1.0.mewoo-plugin`
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`
- `dotnet test Mewoo.slnx --no-restore --verbosity minimal`

## Blocked by

- Issue 043
