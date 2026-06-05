# Issue 066: Plugin Manager Advanced Diagnostic Views

Status: Planned

## What to build

Move runtime diagnostic UI capability into Plugin Manager advanced Sidebar entries and MainArea views.

## Acceptance criteria

- [ ] Plugin Manager Sidebar includes an `Advanced` diagnostics group.
- [ ] The `Advanced` group is collapsed by default.
- [ ] Runtime or discovery issues can surface counts or warning state on the advanced group or related entries.
- [ ] Plugin Manager contributes a `pluginManager.runtimeStatus` MainView for runtime plugin status.
- [ ] Plugin Manager contributes a `pluginManager.discoveryIssues` MainView for discovery issues.
- [ ] Plugin Manager contributes a `pluginManager.operationLog` MainView for package operations and relevant runtime logs.
- [ ] `Open Diagnostics` from Plugin Details opens or focuses the relevant Plugin Manager advanced view.
- [ ] Advanced views may show developer-facing paths, runtime states, low-level messages, and diagnostic actions.
- [ ] Management views keep product-facing language and do not expose raw diagnostic density by default.

## Verification

- Add tests for advanced view contribution, diagnostics navigation, and issue-count presentation where practical.
- Build the solution.

## Notes

- Advanced diagnostics live inside the `Plugins` System Activity Workspace.
- MainArea remains Workbench-hosted MainViews; do not add an internal mode switch or TabControl.
