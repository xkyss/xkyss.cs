# Issue 045: Tag V4 Baseline

## What to build

Prepare and tag the V4 local Plugin Manager baseline.

## Acceptance criteria

- [x] Issues 037-044 are complete.
- [x] Full solution build passes.
- [x] Full test suite passes.
- [x] Manual V4 Plugin Manager checklist is complete.
- [x] `docs/development-plan.md` reflects V4 completion.
- [x] Repository is ready for a `v4-baseline` tag.

## Status

Done.

V4 local Plugin Manager baseline is complete. The final desktop smoke test passed and the repository is ready for the `v4-baseline` tag.

## Verification

- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`
- `dotnet test Mewoo.slnx --no-restore --verbosity minimal`
- Final desktop Plugin Manager smoke test passed.

## Manual V4 Plugin Manager Checklist

- [x] Open the `Plugins` ActivityBar item.
- [x] Confirm installed plugin list and details render without layout issues.
- [x] Use `Install from File` with a valid `.mewoo-plugin` package.
- [x] Confirm install preview shows id, version, publisher, trust warning, and permissions.
- [x] Confirm details `Disable` and `Enable` work.
- [x] Use `Update from File` with a same-id newer package.
- [x] Confirm update preview shows current version versus package version.
- [x] Confirm an id-mismatched update package is blocked.
- [x] Confirm `Uninstall` removes a healthy installed plugin.
- [x] Create or use a broken install directory and confirm `Remove Broken Install` removes it.
- [x] Confirm recent operations show success or failure messages for install, update, uninstall, and broken cleanup.

## Blocked by

- Issue 044
