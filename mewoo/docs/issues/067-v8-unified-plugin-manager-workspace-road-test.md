# Issue 067: Unified Plugin Manager Workspace Road Test

Status: Planned

## What to build

Run an end-to-end V8 road test for the unified Plugin Manager workspace after Runtime Diagnostics is folded into Plugin Manager.

## Acceptance criteria

- [ ] Plugin Manager appears in the ActivityBar System section above Settings.
- [ ] Runtime Diagnostics no longer appears as an ActivityBar entry.
- [ ] Plugin Manager Sidebar management categories drive the home plugin list.
- [ ] Plugin Manager home does not include an internal category TabControl.
- [ ] Plugin Details opens as a dedicated MainArea view.
- [ ] Install and update previews open as dedicated MainArea views.
- [ ] Advanced diagnostics are available from Plugin Manager Sidebar and Plugin Details.
- [ ] Advanced diagnostics default collapsed behavior and issue counts are covered.
- [ ] Runtime status, discovery issues, operation log, install, update, enable, disable, uninstall, and remove broken install flows remain usable.

## Verification

- Add a V8 road-test suite covering the unified Plugin Manager workspace.
- Run the relevant Plugin Manager, Runtime Plugin, Activity Workspace, ActivityBar System section, and Sidebar control tests.
- Build the solution.

## Notes

- This issue should verify behavior and architecture together; it should not introduce new Plugin Manager product features.
