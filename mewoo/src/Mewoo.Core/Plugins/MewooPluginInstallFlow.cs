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

public sealed record MewooPluginUpdatePreview(
    bool Success,
    string PackagePath,
    string InstalledPluginId,
    string? PackagePluginId,
    string? DisplayName,
    string? CurrentVersion,
    string? PackageVersion,
    string VersionComparisonLabel,
    string? Publisher,
    string? PublisherDisplayName,
    string TrustLabel,
    string PermissionSummary,
    IReadOnlyList<string> PermissionLabels,
    string TrustWarning,
    bool PluginIdMatches,
    string Message,
    MewooRuntimePluginIssue? Issue);

public sealed record MewooPluginUpdateResultSummary(
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

public sealed class MewooPluginUpdatePreviewer
{
    private readonly MewooPluginPackageReader _packageReader;

    public MewooPluginUpdatePreviewer(MewooPluginPackageReader? packageReader = null)
    {
        _packageReader = packageReader ?? new MewooPluginPackageReader();
    }

    public MewooPluginUpdatePreview Preview(
        string packagePath,
        MewooPluginManagerCatalogEntry installedEntry)
    {
        if (string.IsNullOrWhiteSpace(installedEntry.PluginId))
        {
            return Failure(
                packagePath,
                installedEntry.PluginId ?? string.Empty,
                null,
                installedEntry.Version,
                "Installed plugin id is unavailable.",
                null,
                pluginIdMatches: false);
        }

        var result = _packageReader.Read(packagePath);
        if (!result.Success || result.Package is null)
        {
            return Failure(
                packagePath,
                installedEntry.PluginId,
                null,
                installedEntry.Version,
                result.Issue?.ShortMessage ?? "Plugin package could not be read.",
                result.Issue,
                pluginIdMatches: false);
        }

        var package = result.Package;
        if (!string.Equals(package.PackageId, installedEntry.PluginId, StringComparison.Ordinal))
        {
            return Failure(
                package.PackagePath,
                installedEntry.PluginId,
                package.PackageId,
                installedEntry.Version,
                $"Package id '{package.PackageId}' does not match installed plugin id '{installedEntry.PluginId}'.",
                new MewooRuntimePluginIssue(
                    MewooRuntimePluginIssueCategory.Package,
                    $"Package id '{package.PackageId}' does not match installed plugin id '{installedEntry.PluginId}'.",
                    package.PackagePath),
                pluginIdMatches: false,
                package.DisplayName,
                package.Version,
                package.Publisher,
                package.PublisherDisplayName,
                package.Manifest);
        }

        var trust = MewooPluginTrustDiagnostics.CreateSummary(package.Manifest);
        return new MewooPluginUpdatePreview(
            true,
            package.PackagePath,
            installedEntry.PluginId,
            package.PackageId,
            package.DisplayName,
            installedEntry.Version,
            package.Version,
            CompareVersions(installedEntry.Version, package.Version),
            package.Publisher,
            package.PublisherDisplayName,
            trust.TrustLabel,
            trust.PermissionSummary,
            trust.PermissionLabels,
            MewooPluginTrustDiagnostics.LocalCodeTrustWarning,
            true,
            "Package is ready to update.",
            null);
    }

    private static MewooPluginUpdatePreview Failure(
        string packagePath,
        string installedPluginId,
        string? packagePluginId,
        string? currentVersion,
        string message,
        MewooRuntimePluginIssue? issue,
        bool pluginIdMatches,
        string? displayName = null,
        string? packageVersion = null,
        string? publisher = null,
        string? publisherDisplayName = null,
        Mewoo.Abstractions.Plugins.MewooPluginManifest? manifest = null)
    {
        var trust = manifest is null
            ? new MewooPluginTrustSummary(
                "Unknown local code",
                "Permission declarations are unavailable because the package manifest could not be read.",
                [],
                false)
            : MewooPluginTrustDiagnostics.CreateSummary(manifest);

        return new MewooPluginUpdatePreview(
            false,
            packagePath,
            installedPluginId,
            packagePluginId,
            displayName,
            currentVersion,
            packageVersion,
            CompareVersions(currentVersion, packageVersion),
            publisher,
            publisherDisplayName,
            trust.TrustLabel,
            trust.PermissionSummary,
            trust.PermissionLabels,
            MewooPluginTrustDiagnostics.LocalCodeTrustWarning,
            pluginIdMatches,
            message,
            issue);
    }

    private static string CompareVersions(string? currentVersion, string? packageVersion)
    {
        if (string.IsNullOrWhiteSpace(currentVersion) || string.IsNullOrWhiteSpace(packageVersion))
        {
            return "Version comparison unavailable";
        }

        if (string.Equals(currentVersion, packageVersion, StringComparison.OrdinalIgnoreCase))
        {
            return "Same version";
        }

        return Version.TryParse(currentVersion, out var current)
            && Version.TryParse(packageVersion, out var package)
            ? package.CompareTo(current) switch
            {
                > 0 => "Newer version",
                < 0 => "Older version",
                _ => "Same version",
            }
            : "Different version";
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

public static class MewooPluginUpdateFlowDisplay
{
    public static MewooPluginUpdateResultSummary CreateResultSummary(MewooPluginOperationResult result)
    {
        return result.Success
            ? new MewooPluginUpdateResultSummary(
                true,
                result.PluginId,
                $"Updated plugin '{result.PluginId}'.",
                result.Path)
            : new MewooPluginUpdateResultSummary(
                false,
                result.PluginId,
                $"{result.Issue?.ShortMessage ?? "Plugin update failed."} Previous installed package was preserved when possible.",
                result.Path);
    }
}
