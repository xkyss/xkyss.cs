# Issue 054: Settings System Activity Skeleton

Status: Done

## What to build

Add a built-in Settings Activity skeleton in the bottom ActivityBar system section.

## Acceptance criteria

- [x] Settings registers as a built-in/system Activity contribution.
- [x] Settings appears at the bottom of the ActivityBar system section below Plugin Manager.
- [x] Settings has its own Sidebar and default MainArea view.
- [x] Settings default view shows basic shell information such as current Mewoo version.
- [x] Settings provides lightweight entry points or placeholders for Appearance, Plugins, and future settings.
- [x] Settings does not introduce a full settings schema or plugin-contributed settings pages in V6.
- [x] Tests cover Settings contribution registration and Activity Workspace behavior.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~MewooBuiltInSystemActivityTests"`

## Notes

- Settings is a system Activity skeleton, not the full settings system.
- Future work may allow plugins to contribute settings pages.

## Blocked by

- Issue 051
- Issue 052
