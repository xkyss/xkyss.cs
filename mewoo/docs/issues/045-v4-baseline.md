# Issue 045: Tag V4 Baseline

## What to build

Prepare and tag the V4 local Plugin Manager baseline.

## Acceptance criteria

- [x] Issues 037-044 are complete.
- [x] Full solution build passes.
- [x] Full test suite passes.
- [ ] Manual V4 Plugin Manager checklist is complete.
- [ ] `docs/development-plan.md` reflects V4 completion.
- [ ] Repository is ready for a `v4-baseline` tag.

## Status

Blocked on final desktop smoke testing.

The code and automated road test are ready for the V4 baseline, but the final manual Plugin Manager checklist still needs to be run in the desktop app before tagging `v4-baseline`.

## Verification

- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`
- `dotnet test Mewoo.slnx --no-restore --verbosity minimal`

## Manual V4 Plugin Manager Checklist

- [ ] Open the `Plugins` ActivityBar item.
- [ ] Confirm installed plugin list and details render without layout issues.
- [ ] Use `Install from File` with a valid `.mewoo-plugin` package.
- [ ] Confirm install preview shows id, version, publisher, trust warning, and permissions.
- [ ] Confirm details `Disable` and `Enable` work.
- [ ] Use `Update from File` with a same-id newer package.
- [ ] Confirm update preview shows current version versus package version.
- [ ] Confirm an id-mismatched update package is blocked.
- [ ] Confirm `Uninstall` removes a healthy installed plugin.
- [ ] Create or use a broken install directory and confirm `Remove Broken Install` removes it.
- [ ] Confirm recent operations show success or failure messages for install, update, uninstall, and broken cleanup.

## Blocked by

- Issue 044
