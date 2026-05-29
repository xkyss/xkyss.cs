# Mewoo V4 Local Plugin Authoring Guide

V4 plugin authoring centers on a local `.mewoo-plugin` package and the Plugin Manager user flow. Runtime Diagnostics remains useful for troubleshooting, but normal install, update, disable, enable, uninstall, and broken install cleanup happen from Plugin Manager.

## Reference Sample

Use the smoke sample as the baseline project shape:

```text
samples/Mewoo.Samples.RuntimeSmokePlugin
```

The plugin project should:

- target `net10.0`;
- reference `Mewoo.Abstractions`;
- reference `Aprillz.MewUI` when it contributes MewUI views;
- avoid references to `Mewoo.App`, `Mewoo.Workbench`, and `Mewoo.Core`.

## Manifest Metadata

Every package contains `mewoo.plugin.json` at the archive root. V4 packages should include publisher, trust, and permission declarations so Plugin Manager can explain risk before install or update.

Example manifest:

```json
{
  "id": "mewoo.samples.runtimeSmoke",
  "displayName": "Runtime Smoke Plugin",
  "version": "0.1.0",
  "assembly": "Mewoo.Samples.RuntimeSmokePlugin.dll",
  "entryPoint": "Mewoo.Samples.RuntimeSmokePlugin.RuntimeSmokePlugin",
  "minimumMewooVersion": "1.0.0",
  "disabled": false,
  "trust": {
    "trustedLocalCode": true,
    "reason": "Built from the in-repo sample project."
  },
  "permissions": [
    {
      "kind": "filesystem",
      "reason": "Reads local sample data during author testing."
    }
  ],
  "metadata": {
    "publisher": "xkyss",
    "publisherDisplayName": "xkyss labs"
  }
}
```

Supported permission kinds are `filesystem`, `processLaunch`, `network`, and `nativeInterop`. Missing permissions are treated conservatively as full local code access.

## Package

Use the local packager to publish the plugin directory and write a `.mewoo-plugin` archive:

```powershell
dotnet run --project tools/Mewoo.PluginPackager/Mewoo.PluginPackager.csproj -- `
  --project samples/Mewoo.Samples.RuntimeSmokePlugin/Mewoo.Samples.RuntimeSmokePlugin.csproj `
  --id mewoo.samples.runtimeSmoke `
  --display-name "Runtime Smoke Plugin" `
  --version 0.1.0 `
  --entry-point Mewoo.Samples.RuntimeSmokePlugin.RuntimeSmokePlugin `
  --configuration Debug `
  --minimum-mewoo-version 1.0.0 `
  --publisher xkyss `
  --publisher-display-name "xkyss labs" `
  --trusted-local-code true `
  --trust-reason "Built from the in-repo sample project." `
  --permissions filesystem `
  --permission-reason "Reads local sample data during author testing." `
  --output-root .build/v4-roadtest/published `
  --package-file .build/v4-roadtest/runtime-smoke-0.1.0.mewoo-plugin
```

The directory under `--output-root` is useful for inspection. The `.mewoo-plugin` file is the artifact to install or update through Plugin Manager.

## Install And Update

Install:

1. Open the `Plugins` ActivityBar item.
2. Choose `Install from File`.
3. Select the `.mewoo-plugin` file.
4. Review id, version, publisher, trust warning, and permissions.
5. Confirm install.

Update:

1. Build a new `.mewoo-plugin` with the same plugin id and a new version.
2. Open the installed plugin details in Plugin Manager.
3. Choose `Update from File`.
4. Confirm that the package id matches and compare current version against package version.
5. Confirm update.

Plugin Manager refuses update packages whose id does not match the installed plugin id. Failed updates report that the previous installed package was preserved when applicable.

## Manage Installed Plugins

From Plugin Manager details:

- use `Disable` and `Enable` for persistent manifest-backed state;
- use `Uninstall` for healthy installed plugins;
- use `Remove Broken Install` for broken directories such as missing manifest, invalid manifest, invalid id, or missing assembly;
- use `Open Diagnostics` when loader state or detailed errors need deeper inspection.

## Safety Notes

V4 local plugins still run as trusted local code inside Mewoo. V4 does not add package signatures, sandbox enforcement, a remote catalog, or marketplace trust policy. Treat `.mewoo-plugin` files like local executable code.
