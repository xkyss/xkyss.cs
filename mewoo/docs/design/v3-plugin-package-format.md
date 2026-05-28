# Mewoo V3 Plugin Package Format

## Purpose

V3 introduces a local installable plugin package so users can install, update, and remove runtime plugins without manually copying directories.

V3 package management builds on the V2 trusted runtime plugin model. It does not make packages untrusted or sandboxed by itself.

## Package Artifact

Package extension:

```text
.mewoo-plugin
```

The artifact is a ZIP archive with a Mewoo-specific extension.

## Archive Layout

The package root must contain:

```text
mewoo.plugin.json
<plugin assembly declared by manifest assembly>
<plugin-private dependencies>
```

Example:

```text
quick-launcher-plus.mewoo-plugin
  mewoo.plugin.json
  QuickLauncherPlus.dll
  QuickLauncherPlus.PrivateDependency.dll
```

`mewoo.plugin.json` must be at the package root. It is not searched from nested directories.

The manifest `assembly` field remains relative to the package root and must point to an archive entry inside the package.

## Metadata

Package identity uses the existing V2 manifest fields:

- `id`: package/plugin id.
- `displayName`: package/plugin display name.
- `version`: package version.

Optional publisher metadata is carried through manifest `metadata`:

```json
{
  "metadata": {
    "publisher": "xkyss",
    "publisherDisplayName": "xkyss labs"
  }
}
```

Packages may also declare local-code trust intent and requested permissions:

```json
{
  "trust": {
    "trustedLocalCode": true,
    "reason": "Built and installed from the local workspace."
  },
  "permissions": [
    {
      "kind": "filesystem",
      "reason": "Reads and writes plugin configuration."
    },
    {
      "kind": "processLaunch",
      "reason": "Runs user-configured commands."
    },
    {
      "kind": "network",
      "reason": "Fetches remote plugin data."
    },
    {
      "kind": "nativeInterop",
      "reason": "Calls a plugin-private native library."
    }
  ]
}
```

If a package omits permission declarations, diagnostics treat that conservatively as undeclared local code access. V3 declarations are visible trust metadata; they are not a sandbox or enforcement boundary by themselves.

Issue 036 will use these declarations as input for the isolation decision. The declarations are informational until an isolation boundary exists.

## Validation Rules

The package reader validates without extracting or modifying the installed plugin root.

Validation fails when:

- the package file is missing;
- the extension is not `.mewoo-plugin`;
- the archive is not a valid ZIP file;
- `mewoo.plugin.json` is missing from the archive root;
- manifest JSON is invalid;
- manifest required fields are missing;
- manifest `id` is invalid;
- manifest `assembly` is rooted or escapes the package root;
- the declared assembly entry is missing.

Validation returns structured package issues so the installer and Runtime Diagnostics can show the failure without partially installing a plugin.

## Relationship to V2 Runtime Plugins

A valid V3 package unwraps to a valid V2 runtime plugin directory:

```text
%LocalAppData%\Mewoo\Plugins\<pluginId>
  mewoo.plugin.json
  <plugin assembly>
  <plugin-private dependencies>
```

Issue 032 owns staging, extraction, replacement, and install failure cleanup.
