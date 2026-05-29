# Mewoo V4 Plugin Manager Product Surface

## Purpose

V4 turns local plugin package management into a user-facing product experience.

Runtime Diagnostics remains the developer and troubleshooting surface. Plugin Manager becomes the normal user entry point for installed local plugins and `.mewoo-plugin` files.

## Product Boundary

Plugin Manager answers:

- What plugins are installed?
- Can I install this local package?
- Is this plugin enabled?
- What version, publisher, trust metadata, and permissions does it declare?
- Can I update or uninstall it?
- What happened during the most recent operation?

Runtime Diagnostics answers:

- What manifest and assembly paths were discovered?
- Was the assembly loaded, registered, activated, unloaded, or failed?
- What low-level runtime issue occurred?
- What logs explain the failure?
- Should a developer load, unload, or reload a plugin during debugging?

Plugin Manager does not expose runtime-only `Load`, `Unload`, or `Reload` actions. It can link to Runtime Diagnostics for advanced troubleshooting.

## Navigation

Plugin Manager is a first-class `Plugins` ActivityBar module.

The module owns:

- a sidebar/list surface for scanning installed plugins;
- a Plugin Manager home MainArea view;
- a Plugin Details MainArea view for one plugin.

It is not nested under the Runtime activity.

## Plugin List

The list supports:

- search by plugin name, id, or publisher;
- status filters for all, enabled, disabled, failed, incompatible, and broken;
- empty state guidance for installing a local `.mewoo-plugin`;
- visible status labels and permission summaries.

The list should show product-level fields:

- display name;
- plugin id;
- version;
- publisher;
- enabled/disabled/runtime problem state;
- permissions summary.

It should not show manifest paths, assembly paths, or install directories by default.

## Plugin Details

The Plugin Details view shows:

- identity: id, display name, version, publisher;
- status: enabled, disabled, loaded, failed, incompatible, or broken;
- trust metadata and local-code warning;
- permissions and permission reasons;
- recent operation results for the current session;
- user-facing error summaries;
- actions: Enable/Disable, Update from File, Uninstall, Open Diagnostics.

Low-level paths belong to Runtime Diagnostics.

## Install from File

Install uses a local file picker for `.mewoo-plugin` files.

Before confirmation, Plugin Manager previews:

- package id;
- display name;
- version;
- publisher;
- trust declaration;
- permissions and reasons;
- local-code trust warning.

V4 warns about high-risk or undeclared permissions, but it does not block installation based on permissions. Permission declarations are explanatory until Mewoo has a real sandbox or signature enforcement model.

## Update from File

Update uses a local file picker from an installed plugin's details view.

Before confirmation, Plugin Manager:

- reads the selected package;
- validates that package id matches the installed plugin id;
- shows current version versus package version;
- previews trust and permissions;
- confirms that failed updates preserve the previous installed package.

The V3 convention of placing `<pluginId>.mewoo-plugin` under the plugin root may remain useful for Runtime Diagnostics or development, but it is not Plugin Manager's primary update path.

## Broken Installs

Plugin Manager shows broken installed plugin entries when a plugin directory exists but cannot become a valid installed plugin entry.

Examples:

- missing `mewoo.plugin.json`;
- invalid manifest JSON;
- invalid plugin id;
- missing manifest-declared assembly.

Broken entries show a user-facing error summary and can be removed from Plugin Manager.

## Recent Operations

Plugin Manager shows current-session operation results for:

- install;
- update;
- enable;
- disable;
- uninstall;
- remove broken install.

V4 does not persist operation history as an audit log.

## Out of Scope

V4 does not include:

- remote plugin catalog;
- marketplace;
- package signatures;
- verified publisher trust;
- sandbox enforcement;
- out-of-process UI plugins.

These remain candidates for later milestones.
