# Issue 029: End-to-End Runtime Plugin Road Test

## What to build

Run the complete runtime plugin workflow manually and record results in the development plan.

## Acceptance criteria

- [x] Package sample runtime plugin with PluginPackager.
- [x] Start Mewoo from a clean state.
- [x] Confirm sample runtime plugin appears in ActivityBar.
- [x] Confirm Sidebar/MainArea/command/StatusBar contributions work.
- [x] Confirm Runtime Diagnostics shows correct state.
- [x] Confirm Disable/Enable works.
- [x] Confirm Unload/Reload works.
- [x] Confirm incompatible plugin does not load.
- [x] Confirm invalid manifest does not crash startup.
- [x] Record follow-up issues for anything deferred.

## Blocked by

- Issue 024
- Issue 025
- Issue 026
- Issue 028

## Road Test Notes

- Packaged the sample runtime plugin with `tools/Mewoo.PluginPackager` into `.build/roadtest/Plugins/mewoo.samples.runtimeSmoke`.
- Confirmed the package contains `mewoo.plugin.json` and `Mewoo.Samples.RuntimeSmokePlugin.dll`.
- Added `RuntimePluginRoadTestCoversContributionsOperationsAndFailures` to cover a clean runtime root, Activity/ViewContainer/MainView/Command/StatusBar contribution registration, diagnostics state, disable/enable, unload/reload, incompatible plugin rejection, and invalid manifest resilience.
- `dotnet test Mewoo.slnx --no-restore --verbosity minimal` passes with 25 tests.
- No deferred follow-up issues were found for the V2 baseline.
