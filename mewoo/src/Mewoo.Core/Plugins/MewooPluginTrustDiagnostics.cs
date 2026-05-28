using Mewoo.Abstractions.Plugins;

namespace Mewoo.Core.Plugins;

public sealed record MewooPluginTrustSummary(
    string TrustLabel,
    string PermissionSummary,
    IReadOnlyList<string> PermissionLabels,
    bool PermissionsDeclared);

public static class MewooPluginTrustDiagnostics
{
    public const string LocalCodeTrustWarning =
        "Local plugin packages run as local code inside Mewoo. Install only packages from sources you trust.";

    public static MewooPluginTrustSummary CreateSummary(MewooPluginManifest manifest)
    {
        var permissions = manifest.Permissions
            .Select(permission => ToPermissionLabel(permission.Kind))
            .ToArray();
        var trustLabel = manifest.Trust?.TrustedLocalCode == true
            ? "Trusted local code declared"
            : "Local code trust not declared";
        var permissionSummary = permissions.Length == 0
            ? "No permissions declared; treat as full local code access."
            : string.Join(", ", permissions);

        return new MewooPluginTrustSummary(
            trustLabel,
            permissionSummary,
            permissions,
            permissions.Length > 0);
    }

    public static string ToPermissionLabel(string kind) =>
        kind switch
        {
            MewooPluginPermissionKinds.Filesystem => "Filesystem",
            MewooPluginPermissionKinds.ProcessLaunch => "Process launch",
            MewooPluginPermissionKinds.Network => "Network",
            MewooPluginPermissionKinds.NativeInterop => "Native interop",
            _ => string.IsNullOrWhiteSpace(kind) ? "Unknown permission" : kind,
        };
}
