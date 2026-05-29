namespace Mewoo.Core.Plugins;

public sealed record MewooPluginManagerCatalogEntry(
    string? PluginId,
    string DisplayName,
    string? Version,
    string? Publisher,
    string? PublisherDisplayName,
    string TrustLabel,
    string PermissionSummary,
    IReadOnlyList<string> PermissionLabels,
    MewooPluginManagerCatalogState State,
    string StateLabel,
    string Message,
    string PluginDirectory);

public enum MewooPluginManagerCatalogState
{
    Enabled,
    Disabled,
    Loaded,
    Failed,
    Incompatible,
    Broken,
}

public enum MewooPluginManagerCatalogFilter
{
    All,
    Enabled,
    Disabled,
    Loaded,
    Failed,
    Incompatible,
    Broken,
}

public sealed record MewooPluginManagerCatalogQuery(
    string? SearchText = null,
    MewooPluginManagerCatalogFilter Filter = MewooPluginManagerCatalogFilter.All);

public sealed class MewooPluginManagerCatalog
{
    private const string InstallStagingDirectoryName = ".install-staging";

    private readonly MewooPluginManifestReader _manifestReader;

    public MewooPluginManagerCatalog(MewooPluginManifestReader? manifestReader = null)
    {
        _manifestReader = manifestReader ?? new MewooPluginManifestReader();
    }

    public IReadOnlyList<MewooPluginManagerCatalogEntry> CreateEntries(
        string pluginRoot,
        IEnumerable<MewooRuntimePluginStatus> runtimeStatuses,
        IEnumerable<MewooRuntimePluginIssue> discoveryIssues,
        MewooPluginManagerCatalogQuery? query = null)
    {
        if (string.IsNullOrWhiteSpace(pluginRoot))
        {
            throw new ArgumentException("Plugin root is required.", nameof(pluginRoot));
        }

        var fullPluginRoot = Path.GetFullPath(pluginRoot);
        var entries = new List<MewooPluginManagerCatalogEntry>();
        var seenDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var status in runtimeStatuses)
        {
            entries.Add(CreateRuntimeEntry(status));
            seenDirectories.Add(Path.GetFullPath(status.Descriptor.PluginDirectory));
        }

        foreach (var issue in discoveryIssues)
        {
            var issueDirectory = IssueDirectory(issue);
            if (seenDirectories.Add(issueDirectory))
            {
                entries.Add(CreateBrokenEntry(issueDirectory, issue.ShortMessage));
            }
        }

        if (Directory.Exists(fullPluginRoot))
        {
            foreach (var directory in Directory.EnumerateDirectories(fullPluginRoot).Order(StringComparer.OrdinalIgnoreCase))
            {
                var fullDirectory = Path.GetFullPath(directory);
                if (string.Equals(Path.GetFileName(fullDirectory), InstallStagingDirectoryName, StringComparison.Ordinal)
                    || !seenDirectories.Add(fullDirectory))
                {
                    continue;
                }

                entries.Add(CreateDirectoryEntry(fullDirectory));
            }
        }

        return ApplyQuery(entries, query).ToArray();
    }

    private static MewooPluginManagerCatalogEntry CreateRuntimeEntry(MewooRuntimePluginStatus status)
    {
        var manifest = status.Descriptor.Manifest;
        var state = ToCatalogState(status);
        var trust = MewooPluginTrustDiagnostics.CreateSummary(manifest);
        return new MewooPluginManagerCatalogEntry(
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
            ToMessage(status, state),
            status.Descriptor.PluginDirectory);
    }

    private MewooPluginManagerCatalogEntry CreateDirectoryEntry(string pluginDirectory)
    {
        var manifestPath = Path.Combine(pluginDirectory, MewooRuntimePluginCatalog.ManifestFileName);
        if (!File.Exists(manifestPath))
        {
            return CreateBrokenEntry(pluginDirectory, "Plugin manifest is missing.");
        }

        MewooRuntimePluginDescriptor descriptor;
        try
        {
            descriptor = _manifestReader.Read(manifestPath);
        }
        catch (Exception ex) when (ex is InvalidOperationException or FileNotFoundException)
        {
            return CreateBrokenEntry(pluginDirectory, ex.Message);
        }

        if (!File.Exists(descriptor.AssemblyPath))
        {
            return CreateBrokenEntry(
                pluginDirectory,
                $"Plugin assembly was not found: {descriptor.Manifest.Assembly}");
        }

        var trust = MewooPluginTrustDiagnostics.CreateSummary(descriptor.Manifest);
        var state = descriptor.Manifest.Disabled
            ? MewooPluginManagerCatalogState.Disabled
            : MewooPluginManagerCatalogState.Enabled;
        return new MewooPluginManagerCatalogEntry(
            descriptor.Manifest.Id,
            descriptor.Manifest.DisplayName,
            descriptor.Manifest.Version,
            TryMetadata(descriptor.Manifest.Metadata, MewooPluginPackageFormat.PublisherMetadataKey),
            TryMetadata(descriptor.Manifest.Metadata, MewooPluginPackageFormat.PublisherDisplayNameMetadataKey),
            trust.TrustLabel,
            trust.PermissionSummary,
            trust.PermissionLabels,
            state,
            ToStateLabel(state),
            descriptor.Manifest.Disabled ? "Plugin is disabled." : "Plugin is installed and enabled.",
            descriptor.PluginDirectory);
    }

    private static MewooPluginManagerCatalogEntry CreateBrokenEntry(string pluginDirectory, string message) =>
        new(
            null,
            Path.GetFileName(pluginDirectory),
            null,
            null,
            null,
            "Unknown local code",
            "Permission declarations are unavailable because the plugin manifest could not be read.",
            [],
            MewooPluginManagerCatalogState.Broken,
            ToStateLabel(MewooPluginManagerCatalogState.Broken),
            message,
            pluginDirectory);

    private static IEnumerable<MewooPluginManagerCatalogEntry> ApplyQuery(
        IEnumerable<MewooPluginManagerCatalogEntry> entries,
        MewooPluginManagerCatalogQuery? query)
    {
        query ??= new MewooPluginManagerCatalogQuery();
        var filtered = entries.Where(entry => MatchesFilter(entry, query.Filter));
        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var searchText = query.SearchText.Trim();
            filtered = filtered.Where(entry => MatchesSearch(entry, searchText));
        }

        return filtered.OrderBy(entry => entry.DisplayName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool MatchesFilter(
        MewooPluginManagerCatalogEntry entry,
        MewooPluginManagerCatalogFilter filter) =>
        filter switch
        {
            MewooPluginManagerCatalogFilter.Enabled => entry.State == MewooPluginManagerCatalogState.Enabled,
            MewooPluginManagerCatalogFilter.Disabled => entry.State == MewooPluginManagerCatalogState.Disabled,
            MewooPluginManagerCatalogFilter.Loaded => entry.State == MewooPluginManagerCatalogState.Loaded,
            MewooPluginManagerCatalogFilter.Failed => entry.State == MewooPluginManagerCatalogState.Failed,
            MewooPluginManagerCatalogFilter.Incompatible => entry.State == MewooPluginManagerCatalogState.Incompatible,
            MewooPluginManagerCatalogFilter.Broken => entry.State == MewooPluginManagerCatalogState.Broken,
            _ => true,
        };

    private static bool MatchesSearch(MewooPluginManagerCatalogEntry entry, string searchText) =>
        Contains(entry.DisplayName, searchText)
        || Contains(entry.PluginId, searchText)
        || Contains(entry.Publisher, searchText)
        || Contains(entry.PublisherDisplayName, searchText);

    private static bool Contains(string? value, string searchText) =>
        value?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true;

    private static string IssueDirectory(MewooRuntimePluginIssue issue)
    {
        var manifestPath = Path.GetFullPath(issue.ManifestPath);
        return Path.GetDirectoryName(manifestPath) ?? manifestPath;
    }

    private static MewooPluginManagerCatalogState ToCatalogState(MewooRuntimePluginStatus status) =>
        status.IssueCategory == MewooRuntimePluginIssueCategory.Assembly
            ? MewooPluginManagerCatalogState.Broken
            : status.State switch
            {
                MewooRuntimePluginState.Disabled => MewooPluginManagerCatalogState.Disabled,
                MewooRuntimePluginState.Incompatible => MewooPluginManagerCatalogState.Incompatible,
                MewooRuntimePluginState.Failed => MewooPluginManagerCatalogState.Failed,
                MewooRuntimePluginState.Loaded or MewooRuntimePluginState.Registered => MewooPluginManagerCatalogState.Loaded,
                _ => MewooPluginManagerCatalogState.Enabled,
            };

    private static string ToMessage(MewooRuntimePluginStatus status, MewooPluginManagerCatalogState state) =>
        string.IsNullOrWhiteSpace(status.Message)
            ? state switch
            {
                MewooPluginManagerCatalogState.Enabled => "Plugin is installed and enabled.",
                MewooPluginManagerCatalogState.Disabled => "Plugin is disabled.",
                MewooPluginManagerCatalogState.Loaded => "Plugin is loaded.",
                MewooPluginManagerCatalogState.Incompatible => "Plugin is incompatible with this Mewoo version.",
                MewooPluginManagerCatalogState.Broken => "Plugin install is broken.",
                _ => status.ShortMessage,
            }
            : status.ShortMessage;

    private static string ToStateLabel(MewooPluginManagerCatalogState state) =>
        state switch
        {
            MewooPluginManagerCatalogState.Enabled => "Enabled",
            MewooPluginManagerCatalogState.Disabled => "Disabled",
            MewooPluginManagerCatalogState.Loaded => "Loaded",
            MewooPluginManagerCatalogState.Failed => "Failed",
            MewooPluginManagerCatalogState.Incompatible => "Incompatible",
            MewooPluginManagerCatalogState.Broken => "Broken",
            _ => "Unknown",
        };

    private static string? TryMetadata(IReadOnlyDictionary<string, string> metadata, string key)
    {
        return metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : null;
    }
}
