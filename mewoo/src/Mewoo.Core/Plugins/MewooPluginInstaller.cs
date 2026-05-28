using System.IO.Compression;

namespace Mewoo.Core.Plugins;

public sealed class MewooPluginInstaller
{
    private const string StagingDirectoryName = ".install-staging";

    private readonly MewooPluginPackageReader _packageReader;

    public MewooPluginInstaller(MewooPluginPackageReader? packageReader = null)
    {
        _packageReader = packageReader ?? new MewooPluginPackageReader();
    }

    public MewooPluginInstallResult Install(
        string packagePath,
        string pluginRoot,
        string? expectedPluginId = null)
    {
        if (string.IsNullOrWhiteSpace(pluginRoot))
        {
            throw new ArgumentException("Plugin root is required.", nameof(pluginRoot));
        }

        var packageResult = _packageReader.Read(packagePath);
        if (!packageResult.Success || packageResult.Package is null)
        {
            return MewooPluginInstallResult.Failed(packageResult.Issue!);
        }

        var package = packageResult.Package;
        if (!string.IsNullOrWhiteSpace(expectedPluginId)
            && !string.Equals(package.PackageId, expectedPluginId, StringComparison.Ordinal))
        {
            return MewooPluginInstallResult.Failed(
                new MewooRuntimePluginIssue(
                    MewooRuntimePluginIssueCategory.Package,
                    $"Plugin package id '{package.PackageId}' does not match expected id '{expectedPluginId}'.",
                    package.PackagePath),
                package.PackageId);
        }

        var fullPluginRoot = Path.GetFullPath(pluginRoot);
        var installPath = Path.Combine(fullPluginRoot, package.PackageId);
        var stagingRoot = Path.Combine(fullPluginRoot, StagingDirectoryName);
        var stagingPath = Path.Combine(stagingRoot, $"{package.PackageId}-{Guid.NewGuid():N}");
        var backupPath = Path.Combine(stagingRoot, $"{package.PackageId}-backup-{Guid.NewGuid():N}");

        try
        {
            Directory.CreateDirectory(stagingRoot);
            Directory.CreateDirectory(stagingPath);
            ExtractPackage(package, stagingPath);
            PromoteStaging(stagingPath, installPath, backupPath);
            return MewooPluginInstallResult.Succeeded(
                package.PackageId,
                installPath,
                MewooPluginTrustDiagnostics.LocalCodeTrustWarning);
        }
        catch (Exception ex) when (ex is IOException
            or UnauthorizedAccessException
            or InvalidDataException
            or InvalidOperationException)
        {
            TryDeleteDirectory(stagingPath);
            return MewooPluginInstallResult.Failed(
                new MewooRuntimePluginIssue(
                    MewooRuntimePluginIssueCategory.Package,
                    ex.Message,
                    package.PackagePath),
                package.PackageId);
        }
        finally
        {
            TryDeleteDirectory(backupPath);
        }
    }

    private static void ExtractPackage(MewooPluginPackageDescriptor package, string stagingPath)
    {
        using var archive = ZipFile.OpenRead(package.PackagePath);
        var fullStagingPath = Path.GetFullPath(stagingPath);
        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Name))
            {
                continue;
            }

            var targetPath = Path.GetFullPath(Path.Combine(fullStagingPath, entry.FullName));
            if (!targetPath.StartsWith(fullStagingPath + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !string.Equals(targetPath, fullStagingPath, StringComparison.Ordinal))
            {
                throw new InvalidDataException($"Plugin package entry '{entry.FullName}' escapes the install directory.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            entry.ExtractToFile(targetPath, overwrite: false);
        }
    }

    private static void PromoteStaging(string stagingPath, string installPath, string backupPath)
    {
        if (Directory.Exists(installPath))
        {
            Directory.Move(installPath, backupPath);
        }

        try
        {
            Directory.Move(stagingPath, installPath);
        }
        catch
        {
            if (Directory.Exists(backupPath) && !Directory.Exists(installPath))
            {
                Directory.Move(backupPath, installPath);
            }

            throw;
        }
    }

    private static void TryDeleteDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        try
        {
            Directory.Delete(path, recursive: true);
        }
        catch
        {
            // Best effort cleanup; the structured install result carries the original failure.
        }
    }
}
