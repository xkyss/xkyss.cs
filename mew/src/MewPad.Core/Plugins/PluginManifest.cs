namespace MewPad.Core.Plugins;

using System.Text.Json;

/// <summary>
/// plugin.json manifest schema.
/// </summary>
public sealed class PluginManifest
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = "0.0.0";
    public string MinHostVersion { get; init; } = "0.0.0";
    public string EntryAssembly { get; init; } = string.Empty;

    public static bool TryLoad(string manifestPath, out PluginManifest? manifest, out string? error)
    {
        manifest = null;
        error = null;

        try
        {
            if (!File.Exists(manifestPath))
            {
                error = $"Manifest not found: {manifestPath}";
                return false;
            }

            var json = File.ReadAllText(manifestPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            manifest = JsonSerializer.Deserialize<PluginManifest>(json, options);
            if (manifest == null)
            {
                error = "Manifest deserialize returned null.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(manifest.Id) || string.IsNullOrWhiteSpace(manifest.EntryAssembly))
            {
                error = "Manifest must define non-empty id and entryAssembly.";
                manifest = null;
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            manifest = null;
            return false;
        }
    }

    public bool IsHostVersionCompatible(string hostVersion)
    {
        if (!TryParseVersionPrefix(MinHostVersion, out var minVersion) ||
            !TryParseVersionPrefix(hostVersion, out var currentVersion))
            return true;

        return currentVersion >= minVersion;
    }

    private static bool TryParseVersionPrefix(string input, out System.Version version)
    {
        version = new System.Version(0, 0, 0);
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var sanitized = input.Trim();
        var dash = sanitized.IndexOf('-');
        if (dash >= 0)
            sanitized = sanitized[..dash];

        return System.Version.TryParse(sanitized, out version!);
    }
}
