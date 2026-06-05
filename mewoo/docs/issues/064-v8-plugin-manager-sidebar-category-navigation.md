# Issue 064: Plugin Manager Sidebar Category Navigation

Status: Planned

## What to build

Move Plugin Manager management categories into its Sidebar navigation and remove duplicate category controls from the Plugin Manager home MainArea view.

## Acceptance criteria

- [ ] Plugin Manager Sidebar has a management section with `All`, `Enabled`, `Disabled`, and `Needs Attention`.
- [ ] `Needs Attention` includes failed, incompatible, broken, or otherwise action-worthy plugin entries.
- [ ] Selecting a management category opens or focuses `pluginManager.home`.
- [ ] `pluginManager.home` renders the selected category's plugin list.
- [ ] Plugin Manager home does not add an internal category `TabControl` or duplicate category button row.
- [ ] Plugin Details remains a dedicated MainArea view opened from a selected plugin row.
- [ ] Install and update previews remain dedicated MainArea views rather than modal dialogs.

## Verification

- Add or update tests for Sidebar category selection and home list filtering.
- Build the solution.

## Notes

- Sidebar navigation owns category selection; Workbench only owns the surrounding Activity Workspace shell state.
