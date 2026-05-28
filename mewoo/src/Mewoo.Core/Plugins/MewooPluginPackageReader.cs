using System.IO.Compression;

namespace Mewoo.Core.Plugins;

public sealed class MewooPluginPackageReader
{
    private readonly MewooPluginManifestReader _manifestReader;

    public MewooPluginPackageReader(MewooPluginManifestReader? manifestReader = null)
    {
        _manifestReader = manifestReader ?? new MewooPluginManifestReader();
    }

    public MewooPluginPackageReadResult Read(string packagePath)
    {
        if (string.IsNullOrWhiteSpace(packagePath))
        {
            throw new ArgumentException("Package path is required.", nameof(packagePath));
        }

        var fullPackagePath = Path.GetFullPath(packagePath);
        if (!File.Exists(fullPackagePath))
        {
            return Failure("Plugin package was not found.", fullPackagePath);
        }

        if (!string.Equals(
            Path.GetExtension(fullPackagePath),
            MewooPluginPackageFormat.Extension,
            StringComparison.OrdinalIgnoreCase))
        {
            return Failure(
                $"Plugin package extension must be '{MewooPluginPackageFormat.Extension}'.",
                fullPackagePath);
        }

        try
        {
            using var archive = ZipFile.OpenRead(fullPackagePath);
            var manifestEntry = FindEntry(archive, MewooPluginPackageFormat.ManifestEntryName);
            if (manifestEntry is null)
            {
                return Failure(
                    $"Plugin package must contain '{MewooPluginPackageFormat.ManifestEntryName}' at the package root.",
                    fullPackagePath);
            }

            var manifestPath = $"{fullPackagePath}!/{MewooPluginPackageFormat.ManifestEntryName}";
            var manifest = ReadManifest(manifestEntry, manifestPath);
            var assemblyEntryName = NormalizeEntryName(manifest.Assembly);
            var assemblyEntry = FindEntry(archive, assemblyEntryName);
            if (assemblyEntry is null)
            {
                return Failure(
                    $"Plugin package must contain assembly '{manifest.Assembly}'.",
                    fullPackagePath,
                    manifest.Assembly,
                    MewooRuntimePluginIssueCategory.Assembly);
            }

            return MewooPluginPackageReadResult.Succeeded(new MewooPluginPackageDescriptor(
                fullPackagePath,
                manifest,
                MewooPluginPackageFormat.ManifestEntryName,
                assemblyEntryName));
        }
        catch (InvalidDataException ex)
        {
            return Failure(ex.Message, fullPackagePath);
        }
        catch (InvalidOperationException ex)
        {
            return Failure(ex.Message, fullPackagePath);
        }
    }

    private Mewoo.Abstractions.Plugins.MewooPluginManifest ReadManifest(
        ZipArchiveEntry manifestEntry,
        string manifestPath)
    {
        using var stream = manifestEntry.Open();
        using var reader = new StreamReader(stream);
        return _manifestReader.ReadManifestJson(reader.ReadToEnd(), manifestPath);
    }

    private static ZipArchiveEntry? FindEntry(ZipArchive archive, string entryName)
    {
        return archive.Entries.FirstOrDefault(entry =>
            string.Equals(NormalizeEntryName(entry.FullName), NormalizeEntryName(entryName), StringComparison.Ordinal));
    }

    private static string NormalizeEntryName(string entryName) =>
        entryName.Replace('\\', '/').TrimStart('/');

    private static MewooPluginPackageReadResult Failure(
        string message,
        string packagePath,
        string? assemblyPath = null,
        MewooRuntimePluginIssueCategory category = MewooRuntimePluginIssueCategory.Package)
    {
        return MewooPluginPackageReadResult.Failed(new MewooRuntimePluginIssue(
            category,
            message,
            packagePath,
            assemblyPath));
    }
}

