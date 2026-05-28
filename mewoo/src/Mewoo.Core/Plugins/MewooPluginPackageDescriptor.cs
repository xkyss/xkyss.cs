using Mewoo.Abstractions.Plugins;

namespace Mewoo.Core.Plugins;

public sealed record MewooPluginPackageDescriptor(
    string PackagePath,
    MewooPluginManifest Manifest,
    string ManifestEntryName,
    string AssemblyEntryName)
{
    public string PackageId => Manifest.Id;

    public string DisplayName => Manifest.DisplayName;

    public string Version => Manifest.Version;

    public string? Publisher => TryMetadata(MewooPluginPackageFormat.PublisherMetadataKey);

    public string? PublisherDisplayName => TryMetadata(MewooPluginPackageFormat.PublisherDisplayNameMetadataKey);

    private string? TryMetadata(string key)
    {
        return Manifest.Metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : null;
    }
}

public sealed record MewooPluginPackageReadResult(
    MewooPluginPackageDescriptor? Package,
    MewooRuntimePluginIssue? Issue)
{
    public bool Success => Package is not null;

    public static MewooPluginPackageReadResult Succeeded(MewooPluginPackageDescriptor package) =>
        new(package, null);

    public static MewooPluginPackageReadResult Failed(MewooRuntimePluginIssue issue) =>
        new(null, issue);
}

