namespace Mewoo.Core.Plugins;

public sealed record MewooPluginCatalogEntry(
    string? PluginId,
    string DisplayName,
    string? Version,
    string? Publisher,
    string? PublisherDisplayName,
    string TrustLabel,
    string PermissionSummary,
    IReadOnlyList<string> PermissionLabels,
    MewooPluginCatalogEntryState State,
    string StateLabel,
    string CategoryLabel,
    string Message,
    string? ManifestPath,
    string? AssemblyPath);

public enum MewooPluginCatalogEntryState
{
    Discovered,
    Installed,
    Disabled,
    Incompatible,
    Failed,
    Loaded,
}

public sealed record MewooPluginCatalogOperation(
    string Operation,
    bool Success,
    string? PluginId,
    string Message,
    string? Path,
    DateTimeOffset Timestamp);

public static class MewooPluginCatalogDisplay
{
    public static IReadOnlyList<MewooPluginCatalogEntry> CreateEntries(
        IEnumerable<MewooRuntimePluginStatus> statuses,
        IEnumerable<MewooRuntimePluginIssue> discoveryIssues)
    {
        var entries = new List<MewooPluginCatalogEntry>();
        entries.AddRange(statuses.Select(CreateEntry));
        entries.AddRange(discoveryIssues.Select(CreateEntry));
        return entries;
    }

    public static MewooPluginCatalogOperation CreateOperation(
        string operation,
        MewooPluginOperationResult result,
        DateTimeOffset timestamp)
    {
        var message = result.Success
            ? $"{operation} succeeded."
            : result.Issue?.ShortMessage ?? $"{operation} failed.";

        return new MewooPluginCatalogOperation(
            operation,
            result.Success,
            result.PluginId,
            message,
            result.Path,
            timestamp);
    }

    private static MewooPluginCatalogEntry CreateEntry(MewooRuntimePluginStatus status)
    {
        var manifest = status.Descriptor.Manifest;
        var state = ToCatalogState(status.State);
        var trust = MewooPluginTrustDiagnostics.CreateSummary(manifest);
        return new MewooPluginCatalogEntry(
            manifest.Id,
            manifest.DisplayName,
            manifest.Version,
            TryMetadata(manifest.Metadata, MewooPluginPackageFormat.PublisherMetadataKey),
            TryMetadata(manifest.Metadata, MewooPluginPackageFormat.PublisherDisplayNameMetadataKey),
            trust.TrustLabel,
            trust.PermissionSummary,
            trust.PermissionLabels,
            state,
            ToStateLabel(state),
            status.CategoryLabel,
            status.ShortMessage,
            status.Descriptor.ManifestPath,
            status.Descriptor.AssemblyPath);
    }

    private static MewooPluginCatalogEntry CreateEntry(MewooRuntimePluginIssue issue)
    {
        return new MewooPluginCatalogEntry(
            null,
            "Discovered plugin issue",
            null,
            null,
            null,
            "Unknown local code",
            "Permission declarations are unavailable because the manifest could not be read.",
            [],
            MewooPluginCatalogEntryState.Discovered,
            ToStateLabel(MewooPluginCatalogEntryState.Discovered),
            issue.Category.ToString(),
            issue.ShortMessage,
            issue.ManifestPath,
            issue.AssemblyPath);
    }

    private static MewooPluginCatalogEntryState ToCatalogState(MewooRuntimePluginState state) =>
        state switch
        {
            MewooRuntimePluginState.Disabled => MewooPluginCatalogEntryState.Disabled,
            MewooRuntimePluginState.Incompatible => MewooPluginCatalogEntryState.Incompatible,
            MewooRuntimePluginState.Failed => MewooPluginCatalogEntryState.Failed,
            MewooRuntimePluginState.Loaded or MewooRuntimePluginState.Registered => MewooPluginCatalogEntryState.Loaded,
            _ => MewooPluginCatalogEntryState.Installed,
        };

    private static string ToStateLabel(MewooPluginCatalogEntryState state) =>
        state switch
        {
            MewooPluginCatalogEntryState.Discovered => "Discovered",
            MewooPluginCatalogEntryState.Installed => "Installed",
            MewooPluginCatalogEntryState.Disabled => "Disabled",
            MewooPluginCatalogEntryState.Incompatible => "Incompatible",
            MewooPluginCatalogEntryState.Failed => "Failed",
            MewooPluginCatalogEntryState.Loaded => "Loaded",
            _ => "Unknown",
        };

    private static string? TryMetadata(IReadOnlyDictionary<string, string> metadata, string key)
    {
        return metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : null;
    }
}
