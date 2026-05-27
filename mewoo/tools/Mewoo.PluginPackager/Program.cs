using System.Diagnostics;
using System.Text.Json;
using Mewoo.Abstractions.Plugins;
using Mewoo.Core.Plugins;

var options = PackageOptions.Parse(args);
if (options is null)
{
    PackageOptions.PrintUsage();
    return 2;
}

var exitCode = await PackageAsync(options);
return exitCode;

static async Task<int> PackageAsync(PackageOptions options)
{
    var projectPath = Path.GetFullPath(options.ProjectPath);
    if (!File.Exists(projectPath))
    {
        Console.Error.WriteLine($"Project not found: {projectPath}");
        return 1;
    }

    var packageRoot = options.OutputRoot is null
        ? GetDefaultPluginRoot()
        : Path.GetFullPath(options.OutputRoot);
    var packageDirectory = Path.Combine(packageRoot, options.PluginId);

    if (Directory.Exists(packageDirectory))
    {
        Directory.Delete(packageDirectory, recursive: true);
    }

    Directory.CreateDirectory(packageDirectory);

    var publishExitCode = await RunDotnetPublishAsync(projectPath, options.Configuration, packageDirectory);
    if (publishExitCode != 0)
    {
        return publishExitCode;
    }

    RemoveHostSharedAssemblies(packageDirectory);
    WriteManifest(options, projectPath, packageDirectory);
    Console.WriteLine($"Packaged runtime plugin '{options.PluginId}' to {packageDirectory}");
    return 0;
}

static async Task<int> RunDotnetPublishAsync(string projectPath, string configuration, string packageDirectory)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = "dotnet",
        RedirectStandardError = true,
        RedirectStandardOutput = true,
    };
    startInfo.ArgumentList.Add("publish");
    startInfo.ArgumentList.Add(projectPath);
    startInfo.ArgumentList.Add("-c");
    startInfo.ArgumentList.Add(configuration);
    startInfo.ArgumentList.Add("-o");
    startInfo.ArgumentList.Add(packageDirectory);

    using var process = Process.Start(startInfo);
    if (process is null)
    {
        Console.Error.WriteLine("Failed to start dotnet publish.");
        return 1;
    }

    await process.WaitForExitAsync();
    Console.Write(await process.StandardOutput.ReadToEndAsync());
    Console.Error.Write(await process.StandardError.ReadToEndAsync());
    return process.ExitCode;
}

static void RemoveHostSharedAssemblies(string packageDirectory)
{
    foreach (var file in Directory.EnumerateFiles(packageDirectory))
    {
        var name = Path.GetFileName(file);
        if (name.StartsWith("Mewoo.Abstractions.", StringComparison.OrdinalIgnoreCase)
            || name.StartsWith("Aprillz.MewUI", StringComparison.OrdinalIgnoreCase))
        {
            File.Delete(file);
        }
    }
}

static void WriteManifest(PackageOptions options, string projectPath, string packageDirectory)
{
    var assemblyName = Path.GetFileNameWithoutExtension(projectPath) + ".dll";
    var manifest = new MewooPluginManifest
    {
        Id = options.PluginId,
        DisplayName = options.DisplayName,
        Version = options.Version,
        Assembly = assemblyName,
        EntryPoint = options.EntryPoint,
        MinimumMewooVersion = options.MinimumMewooVersion,
        Disabled = false,
    };

    var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    });
    File.WriteAllText(Path.Combine(packageDirectory, MewooRuntimePluginCatalog.ManifestFileName), json);
}

static string GetDefaultPluginRoot()
{
    var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    return Path.Combine(root, "Mewoo", "Plugins");
}

internal sealed record PackageOptions(
    string ProjectPath,
    string PluginId,
    string DisplayName,
    string Version,
    string EntryPoint,
    string Configuration,
    string? OutputRoot,
    string? MinimumMewooVersion)
{
    public static PackageOptions? Parse(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                return null;
            }

            if (index + 1 >= args.Length)
            {
                return null;
            }

            values[arg[2..]] = args[++index];
        }

        return Required(values, "project") is { } project
            && Required(values, "id") is { } id
            && Required(values, "display-name") is { } displayName
            && Required(values, "version") is { } version
            && Required(values, "entry-point") is { } entryPoint
                ? new PackageOptions(
                    project,
                    id,
                    displayName,
                    version,
                    entryPoint,
                    values.GetValueOrDefault("configuration", "Release"),
                    values.GetValueOrDefault("output-root"),
                    values.GetValueOrDefault("minimum-mewoo-version"))
                : null;
    }

    public static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run --project tools/Mewoo.PluginPackager -- --project <plugin.csproj> --id <pluginId> --display-name <name> --version <version> --entry-point <type> [--output-root <dir>] [--configuration Release] [--minimum-mewoo-version 1.0.0]");
    }

    private static string? Required(IReadOnlyDictionary<string, string> values, string key)
    {
        return values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : null;
    }
}
