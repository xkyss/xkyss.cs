using Mewoo.Abstractions.Logging;

namespace Mewoo.Core.Plugins;

public sealed class MewooRuntimePluginCatalog
{
    public const string ManifestFileName = "mewoo.plugin.json";

    private readonly MewooPluginManifestReader _reader;
    private readonly IMewooLogger? _logger;
    private readonly List<MewooRuntimePluginIssue> _discoveryIssues = [];

    public MewooRuntimePluginCatalog(IMewooLogger? logger = null, MewooPluginManifestReader? reader = null)
    {
        _logger = logger;
        _reader = reader ?? new MewooPluginManifestReader();
    }

    public IReadOnlyList<MewooRuntimePluginIssue> DiscoveryIssues => _discoveryIssues;

    public IReadOnlyList<MewooRuntimePluginDescriptor> Discover(string pluginRoot)
    {
        if (string.IsNullOrWhiteSpace(pluginRoot))
        {
            throw new ArgumentException("Plugin root is required.", nameof(pluginRoot));
        }

        _discoveryIssues.Clear();
        var fullRoot = Path.GetFullPath(pluginRoot);
        if (!Directory.Exists(fullRoot))
        {
            _logger?.Info("RuntimePluginCatalog", $"Runtime plugin directory does not exist: {fullRoot}");
            return [];
        }

        var descriptors = new List<MewooRuntimePluginDescriptor>();
        foreach (var pluginDirectory in Directory.EnumerateDirectories(fullRoot).Order(StringComparer.OrdinalIgnoreCase))
        {
            var manifestPath = Path.Combine(pluginDirectory, ManifestFileName);
            if (!File.Exists(manifestPath))
            {
                continue;
            }

            try
            {
                var descriptor = _reader.Read(manifestPath);
                if (descriptor.Manifest.Disabled)
                {
                    _logger?.Info("RuntimePluginCatalog", $"Discovered disabled runtime plugin '{descriptor.Manifest.Id}'.");
                }
                else
                {
                    _logger?.Info("RuntimePluginCatalog", $"Discovered runtime plugin '{descriptor.Manifest.Id}'.");
                }

                descriptors.Add(descriptor);
            }
            catch (Exception ex)
            {
                _discoveryIssues.Add(new MewooRuntimePluginIssue(
                    MewooRuntimePluginIssueCategory.Manifest,
                    ex.Message,
                    manifestPath));
                _logger?.Error("RuntimePluginCatalog", $"Failed to read runtime plugin manifest '{manifestPath}'.", ex);
            }
        }

        return descriptors;
    }
}
