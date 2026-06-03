# Issue 054: Settings System Activity Skeleton

## What to build

Add a built-in Settings Activity skeleton in the bottom ActivityBar system section.

## Acceptance criteria

- [ ] Settings registers as a built-in/system Activity contribution.
- [ ] Settings appears at the bottom of the ActivityBar system section below Plugin Manager.
- [ ] Settings has its own Sidebar and default MainArea view.
- [ ] Settings default view shows basic shell information such as current Mewoo version.
- [ ] Settings provides lightweight entry points or placeholders for Appearance, Plugins, and future settings.
- [ ] Settings does not introduce a full settings schema or plugin-contributed settings pages in V6.
- [ ] Tests cover Settings contribution registration and Activity Workspace behavior.

## Notes

- Settings is a system Activity skeleton, not the full settings system.
- Future work may allow plugins to contribute settings pages.

## Blocked by

- Issue 051
- Issue 052
