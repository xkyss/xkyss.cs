using System.Diagnostics;
using System.IO.Compression;
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
    if (options.PackageFile is not null
        && !string.Equals(
            Path.GetExtension(options.PackageFile),
            MewooPluginPackageFormat.Extension,
            StringComparison.OrdinalIgnoreCase))
    {
        Console.Error.WriteLine($"Package file extension must be '{MewooPluginPackageFormat.Extension}'.");
        return 1;
    }

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
    if (options.PackageFile is not null)
    {
        CreatePackageFile(packageDirectory, options.PackageFile);
        Console.WriteLine($"Wrote plugin package '{Path.GetFullPath(options.PackageFile)}'");
    }

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
    var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
    if (!string.IsNullOrWhiteSpace(options.Publisher))
    {
        metadata[MewooPluginPackageFormat.PublisherMetadataKey] = options.Publisher;
    }

    if (!string.IsNullOrWhiteSpace(options.PublisherDisplayName))
    {
        metadata[MewooPluginPackageFormat.PublisherDisplayNameMetadataKey] = options.PublisherDisplayName;
    }

    var manifest = new MewooPluginManifest
    {
        Id = options.PluginId,
        DisplayName = options.DisplayName,
        Version = options.Version,
        Assembly = assemblyName,
        EntryPoint = options.EntryPoint,
        MinimumMewooVersion = options.MinimumMewooVersion,
        Disabled = false,
        Trust = options.TrustedLocalCode.HasValue || !string.IsNullOrWhiteSpace(options.TrustReason)
            ? new MewooPluginTrustDeclaration
            {
                TrustedLocalCode = options.TrustedLocalCode == true,
                Reason = options.TrustReason,
            }
            : null,
        Permissions = options.Permissions
            .Select(permission => new MewooPluginPermissionDeclaration
            {
                Kind = permission,
                Reason = options.PermissionReason,
            })
            .ToArray(),
        Metadata = metadata,
    };

    var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    });
    File.WriteAllText(Path.Combine(packageDirectory, MewooRuntimePluginCatalog.ManifestFileName), json);
}

static void CreatePackageFile(string packageDirectory, string packageFile)
{
    var fullPackageFile = Path.GetFullPath(packageFile);
    Directory.CreateDirectory(Path.GetDirectoryName(fullPackageFile)!);
    if (File.Exists(fullPackageFile))
    {
        File.Delete(fullPackageFile);
    }

    ZipFile.CreateFromDirectory(
        packageDirectory,
        fullPackageFile,
        CompressionLevel.Optimal,
        includeBaseDirectory: false);
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
    string? PackageFile,
    string? MinimumMewooVersion,
    string? Publisher,
    string? PublisherDisplayName,
    bool? TrustedLocalCode,
    string? TrustReason,
    IReadOnlyList<string> Permissions,
    string? PermissionReason)
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
                    values.GetValueOrDefault("package-file"),
                    values.GetValueOrDefault("minimum-mewoo-version"),
                    values.GetValueOrDefault("publisher"),
                    values.GetValueOrDefault("publisher-display-name"),
                    ParseOptionalBool(values.GetValueOrDefault("trusted-local-code")),
                    values.GetValueOrDefault("trust-reason"),
                    ParsePermissions(values.GetValueOrDefault("permissions")),
                    values.GetValueOrDefault("permission-reason"))
                : null;
    }

    public static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run --project tools/Mewoo.PluginPackager -- --project <plugin.csproj> --id <pluginId> --display-name <name> --version <version> --entry-point <type> [--output-root <dir>] [--package-file <plugin.mewoo-plugin>] [--configuration Release] [--minimum-mewoo-version 1.0.0] [--publisher <id>] [--publisher-display-name <name>] [--trusted-local-code true] [--trust-reason <text>] [--permissions filesystem,network] [--permission-reason <text>]");
    }

    private static string? Required(IReadOnlyDictionary<string, string> values, string key)
    {
        return values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : null;
    }

    private static bool? ParseOptionalBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return bool.TryParse(value, out var parsed) ? parsed : null;
    }

    private static IReadOnlyList<string> ParsePermissions(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
