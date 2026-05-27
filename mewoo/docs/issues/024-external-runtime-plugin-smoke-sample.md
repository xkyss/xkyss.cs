# Issue 024: Create an External Runtime Plugin Smoke Sample

## What to build

Create a small external runtime plugin sample that is packaged into `%LocalAppData%\Mewoo\Plugins` and loaded by Mewoo without being registered directly by `Mewoo.App`.

## Acceptance criteria

- [ ] Sample plugin is not registered as a compiled plugin by `Mewoo.App`.
- [ ] Sample plugin package includes `mewoo.plugin.json`.
- [ ] Sample plugin contributes an ActivityBar item.
- [ ] Sample plugin contributes a Sidebar view.
- [ ] Sample plugin contributes a MainArea view.
- [ ] Sample plugin contributes a command.
- [ ] Sample plugin contributes a StatusBar item.
- [ ] Mewoo startup loads and activates the packaged sample plugin from the runtime plugin directory.

## Blocked by

- Slice 023

