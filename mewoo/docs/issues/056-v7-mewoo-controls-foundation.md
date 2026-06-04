# Issue 056: Mewoo.Controls Project and Sidebar Helper Foundation

Status: Done

## What to build

Add a `Mewoo.Controls` project for optional plugin UI helpers and wire built-in plugin projects so they can use it.

## Acceptance criteria

- [x] `Mewoo.Controls` exists as a separate project.
- [x] `Mewoo.Controls` references `Mewoo.Abstractions` and `Aprillz.MewUI`.
- [x] `Mewoo.Controls` does not reference `Mewoo.Workbench`.
- [x] Built-in plugins can optionally reference `Mewoo.Controls`.
- [x] V7 scope is limited to Sidebar helpers and the small common controls required by those helpers.
- [x] Runtime/local plugin manifests do not gain a required controls dependency declaration.

## Verification

- Restored `src\Mewoo.Controls\Mewoo.Controls.csproj` from the local NuGet package cache because `Aprillz.MewUI` is not reachable from nuget.org in this environment.
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`
- Result: passed. Existing NU1900 and MSTEST0037 warnings remain.

## Notes

- `Mewoo.Controls` is a helper library, not a required contribution schema.
- V7 common controls include only the primitives needed by Sidebar helpers, such as icon-style action buttons, search input, navigation rows, and empty/placeholder blocks.

## Blocked by

- ADR 0017
