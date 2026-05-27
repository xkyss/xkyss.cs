using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mewoo.Plugins.QuickLauncher;

public sealed class QuickLauncherConfig
{
    public List<QuickLauncherGroup> Groups { get; init; } = [];

    public static QuickLauncherConfig LoadOrCreateDefault()
    {
        var path = GetDefaultConfigPath();
        if (!File.Exists(path))
        {
            var config = CreateDefault();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(config, JsonOptions));
            return config;
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<QuickLauncherConfig>(json, JsonOptions) ?? CreateDefault();
    }

    public static string GetDefaultConfigPath()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(root, "Mewoo", "QuickLauncher", "launcher.json");
    }

    private static QuickLauncherConfig CreateDefault()
    {
        return new QuickLauncherConfig
        {
            Groups =
            [
                new QuickLauncherGroup
                {
                    Id = "development",
                    Title = "Development",
                    Items =
                    [
                        new QuickLauncherItem
                        {
                            Id = "github",
                            Title = "GitHub",
                            Kind = QuickLauncherItemKind.Url,
                            Target = "https://github.com",
                            Tags = ["code"],
                        },
                        new QuickLauncherItem
                        {
                            Id = "terminal",
                            Title = "Terminal",
                            Kind = QuickLauncherItemKind.Executable,
                            Target = "wt.exe",
                            Tags = ["shell"],
                        },
                        new QuickLauncherItem
                        {
                            Id = "build-script",
                            Title = "Build Script",
                            Kind = QuickLauncherItemKind.Script,
                            Target = "scripts/build.ps1",
                            Tags = ["script"],
                        },
                    ],
                },
                new QuickLauncherGroup
                {
                    Id = "trading",
                    Title = "Trading",
                    Items =
                    [
                        new QuickLauncherItem
                        {
                            Id = "binance",
                            Title = "Binance",
                            Kind = QuickLauncherItemKind.Url,
                            Target = "https://www.binance.com",
                            Tags = ["market"],
                        },
                    ],
                },
            ],
        };
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };
}

public sealed class QuickLauncherGroup
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public List<QuickLauncherItem> Items { get; init; } = [];
}

public sealed class QuickLauncherItem
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public required QuickLauncherItemKind Kind { get; init; }

    public required string Target { get; init; }

    public string? Arguments { get; init; }

    public string? Icon { get; init; }

    public List<string> Tags { get; init; } = [];
}

public enum QuickLauncherItemKind
{
    Url,
    File,
    Executable,
    Script,
}

