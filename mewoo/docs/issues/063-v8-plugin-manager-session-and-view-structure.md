# Issue 063: Plugin Manager Session and View Structure

Status: Planned

## What to build

Split the current Plugin Manager implementation into a small registration plugin plus explicit session, action/controller, and view classes without changing user-visible behavior yet.

## Acceptance criteria

- [ ] `PluginManagerPlugin` primarily registers contributions and creates Plugin Manager views.
- [ ] Plugin Manager session state is represented explicitly: selected category, search text, selected plugin key, install preview, update preview, and recent operation results.
- [ ] Plugin Manager actions/controller code owns install, update, enable, disable, uninstall, remove broken install, diagnostics navigation, and post-action session updates.
- [ ] Sidebar, home, details, install preview, and update preview rendering are moved out of the root plugin class.
- [ ] Existing Plugin Manager install, update, enable, disable, uninstall, broken install removal, and details flows keep their behavior.
- [ ] Workbench state ownership is unchanged; Plugin Manager session state is not moved into Workbench persistence.

## Verification

- Run existing Plugin Manager catalog, package operation, and V4 road-test coverage.
- Build the solution.

## Notes

- This is a structure slice before V8 changes the navigation model.
- Do not fold Runtime Diagnostics in this issue.
