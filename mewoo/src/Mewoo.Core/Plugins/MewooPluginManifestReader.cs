using System.Text.Json;
using System.Text.RegularExpressions;
using Mewoo.Abstractions.Plugins;

namespace Mewoo.Core.Plugins;

public sealed partial class MewooPluginManifestReader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public MewooRuntimePluginDescriptor Read(string manifestPath)
    {
        if (string.IsNullOrWhiteSpace(manifestPath))
        {
            throw new ArgumentException("Manifest path is required.", nameof(manifestPath));
        }

        var fullManifestPath = Path.GetFullPath(manifestPath);
        if (!File.Exists(fullManifestPath))
        {
            throw new FileNotFoundException("Plugin manifest was not found.", fullManifestPath);
        }

        var json = File.ReadAllText(fullManifestPath);
        MewooPluginManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<MewooPluginManifest>(json, JsonOptions)
                ?? throw new InvalidOperationException($"Plugin manifest '{fullManifestPath}' is empty.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Plugin manifest '{fullManifestPath}' is invalid: {ex.Message}", ex);
        }

        Validate(manifest, fullManifestPath);

        var pluginDirectory = Path.GetDirectoryName(fullManifestPath)
            ?? throw new InvalidOperationException($"Plugin manifest '{fullManifestPath}' has no containing directory.");
        var assemblyPath = Path.GetFullPath(Path.Combine(pluginDirectory, manifest.Assembly));

        return new MewooRuntimePluginDescriptor(
            manifest,
            fullManifestPath,
            pluginDirectory,
            assemblyPath);
    }

    private static void Validate(MewooPluginManifest manifest, string manifestPath)
    {
        RequirePluginId(manifest.Id, manifestPath);
        RequireText(manifest.DisplayName, nameof(manifest.DisplayName), manifestPath);
        RequireText(manifest.Version, nameof(manifest.Version), manifestPath);
        RequireText(manifest.Assembly, nameof(manifest.Assembly), manifestPath);
        RequireText(manifest.EntryPoint, nameof(manifest.EntryPoint), manifestPath);

        if (Path.IsPathRooted(manifest.Assembly))
        {
            throw new InvalidOperationException($"Plugin manifest '{manifestPath}' assembly path must be relative.");
        }

        if (manifest.Assembly.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(part => part == ".."))
        {
            throw new InvalidOperationException($"Plugin manifest '{manifestPath}' assembly path must stay inside the plugin directory.");
        }
    }

    private static void RequirePluginId(string? value, string manifestPath)
    {
        RequireText(value, nameof(MewooPluginManifest.Id), manifestPath);
        if (!PluginIdRegex().IsMatch(value!))
        {
            throw new InvalidOperationException($"Plugin manifest '{manifestPath}' has invalid plugin id '{value}'.");
        }
    }

    private static void RequireText(string? value, string fieldName, string manifestPath)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Plugin manifest '{manifestPath}' is missing '{fieldName}'.");
        }
    }

    [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9]*(\.[A-Za-z][A-Za-z0-9]*)*$", RegexOptions.CultureInvariant)]
    private static partial Regex PluginIdRegex();
}
