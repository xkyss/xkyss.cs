# Issue 063: Plugin Manager Session and View Structure

Status: Done

## What to build

Split the current Plugin Manager implementation into a small registration plugin plus explicit session, action/controller, and view classes without changing user-visible behavior yet.

## Acceptance criteria

- [x] `PluginManagerPlugin` primarily registers contributions and creates Plugin Manager views.
- [x] Plugin Manager session state is represented explicitly: selected category, search text, selected plugin key, install preview, update preview, and recent operation results.
- [x] Plugin Manager actions/controller code owns install, update, enable, disable, uninstall, remove broken install, diagnostics navigation, and post-action session updates.
- [x] Sidebar, home, details, install preview, and update preview rendering are moved out of the root plugin class.
- [x] Existing Plugin Manager install, update, enable, disable, uninstall, broken install removal, and details flows keep their behavior.
- [x] Workbench state ownership is unchanged; Plugin Manager session state is not moved into Workbench persistence.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~PluginManager|FullyQualifiedName~PluginPackageOperations"`
- Result: passed, 20 tests.
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`
- Result: passed. Existing NU1900 and MSTEST0037 warnings remain.

## Notes

- This is a structure slice before V8 changes the navigation model.
- Do not fold Runtime Diagnostics in this issue.
