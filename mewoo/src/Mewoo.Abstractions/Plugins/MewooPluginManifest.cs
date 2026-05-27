namespace Mewoo.Abstractions.Plugins;

public sealed record MewooPluginManifest
{
    public required string Id { get; init; }

    public required string DisplayName { get; init; }

    public required string Version { get; init; }

    public required string Assembly { get; init; }

    public required string EntryPoint { get; init; }

    public string? MinimumMewooVersion { get; init; }

    public bool Disabled { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}

