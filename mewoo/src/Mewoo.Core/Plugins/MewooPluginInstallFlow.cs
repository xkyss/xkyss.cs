namespace Mewoo.Core.Plugins;

public sealed record MewooPluginInstallPreview(
    bool Success,
    string PackagePath,
    string? PluginId,
    string? DisplayName,
    string? Version,
    string? Publisher,
    string? PublisherDisplayName,
    string TrustLabel,
    string PermissionSummary,
    IReadOnlyList<string> PermissionLabels,
    string TrustWarning,
    MewooRuntimePluginIssue? Issue);

public sealed record MewooPluginInstallResultSummary(
    bool Success,
    string? PluginId,
    string Message,
    string? InstalledPath);

public sealed class MewooPluginInstallPreviewer
{
    private readonly MewooPluginPackageReader _packageReader;

    public MewooPluginInstallPreviewer(MewooPluginPackageReader? packageReader = null)
    {
        _packageReader = packageReader ?? new MewooPluginPackageReader();
    }

    public MewooPluginInstallPreview Preview(string packagePath)
    {
        var result = _packageReader.Read(packagePath);
        if (!result.Success || result.Package is null)
        {
            return new MewooPluginInstallPreview(
                false,
                packagePath,
                null,
                null,
                null,
                null,
                null,
                "Unknown local code",
                "Permission declarations are unavailable because the package manifest could not be read.",
                [],
                MewooPluginTrustDiagnostics.LocalCodeTrustWarning,
                result.Issue);
        }

        var package = result.Package;
        var trust = MewooPluginTrustDiagnostics.CreateSummary(package.Manifest);
        return new MewooPluginInstallPreview(
            true,
            package.PackagePath,
            package.PackageId,
            package.DisplayName,
            package.Version,
            package.Publisher,
            package.PublisherDisplayName,
            trust.TrustLabel,
            trust.PermissionSummary,
            trust.PermissionLabels,
            MewooPluginTrustDiagnostics.LocalCodeTrustWarning,
            null);
    }
}

public static class MewooPluginInstallFlowDisplay
{
    public static MewooPluginInstallResultSummary CreateResultSummary(MewooPluginInstallResult result)
    {
        return result.Success
            ? new MewooPluginInstallResultSummary(
                true,
                result.PluginId,
                $"Installed plugin '{result.PluginId}'.",
                result.InstalledPath)
            : new MewooPluginInstallResultSummary(
                false,
                result.PluginId,
                result.Issue?.ShortMessage ?? "Plugin install failed.",
                result.InstalledPath);
    }
}
