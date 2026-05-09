namespace MewPad.Hosting.Plugins;

using System.Reflection;
using System.Runtime.Loader;
using MewPad.Core.Plugins;
using MewPad.Core.Shell;

/// <summary>
/// Minimal runtime plugin loader.
/// Supports two conventions:
/// 1) Types implementing IPlugin
/// 2) Public static Register(ShellContext shell) methods
/// </summary>
internal static class PluginLoader
{
    public static PluginLoadSummary LoadFromDirectory(ShellContext shell, string pluginsDirectory, PluginLoadOptions? options = null)
    {
        options ??= new PluginLoadOptions();

        if (!Directory.Exists(pluginsDirectory))
            return new PluginLoadSummary(pluginsDirectory, 0, 0, [], []);

        var loadedPlugins = 0;
        var failedPlugins = 0;
        var errors = new List<string>();
        var items = new List<PluginLoadItem>();

        // Manifest-based plugins: plugins/<plugin>/plugin.json
        foreach (var pluginDir in Directory.GetDirectories(pluginsDirectory))
        {
            var manifestPath = Path.Combine(pluginDir, "plugin.json");
            if (!File.Exists(manifestPath))
                continue;

            if (!PluginManifest.TryLoad(manifestPath, out var manifest, out var manifestError) || manifest == null)
            {
                failedPlugins++;
                var msg = $"Manifest parse failed: {Path.GetFileName(pluginDir)} => {manifestError}";
                errors.Add(msg);
                items.Add(new PluginLoadItem(Path.GetFileName(pluginDir), Path.GetFileName(pluginDir), "unknown", false, false, manifestError ?? "manifest parse failed"));
                continue;
            }

            if (options.DisabledPluginIds.Contains(manifest.Id))
            {
                items.Add(new PluginLoadItem(manifest.Id, manifest.Name, manifest.Version, false, false, null));
                continue;
            }

            if (!manifest.IsHostVersionCompatible(options.HostVersion))
            {
                failedPlugins++;
                var msg = $"Version incompatible: {manifest.Id} requires >= {manifest.MinHostVersion}, host={options.HostVersion}";
                errors.Add(msg);
                items.Add(new PluginLoadItem(manifest.Id, manifest.Name, manifest.Version, true, false, msg));
                continue;
            }

            var assemblyPath = Path.Combine(pluginDir, manifest.EntryAssembly);
            if (!File.Exists(assemblyPath))
            {
                failedPlugins++;
                var msg = $"Entry assembly missing: {manifest.Id} => {assemblyPath}";
                errors.Add(msg);
                items.Add(new PluginLoadItem(manifest.Id, manifest.Name, manifest.Version, true, false, msg));
                continue;
            }

            try
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(assemblyPath));
                var count = RegisterFromAssembly(shell, assembly, assemblyPath, errors);
                if (count > 0)
                {
                    loadedPlugins += count;
                    items.Add(new PluginLoadItem(manifest.Id, manifest.Name, manifest.Version, true, true, null));
                }
                else
                {
                    failedPlugins++;
                    var msg = $"No valid entrypoint found: {manifest.Id}";
                    errors.Add(msg);
                    items.Add(new PluginLoadItem(manifest.Id, manifest.Name, manifest.Version, true, false, msg));
                }
            }
            catch (Exception ex)
            {
                failedPlugins++;
                var msg = $"Load failed: {manifest.Id} => {ex.Message}";
                errors.Add(msg);
                items.Add(new PluginLoadItem(manifest.Id, manifest.Name, manifest.Version, true, false, msg));
            }
        }

        // Backward-compatible direct DLL scan in top-level plugins directory.
        var dllFiles = Directory.GetFiles(pluginsDirectory, "*.dll", SearchOption.TopDirectoryOnly);

        foreach (var dllPath in dllFiles)
        {
            try
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(dllPath));
                var count = RegisterFromAssembly(shell, assembly, dllPath, errors);
                loadedPlugins += count;
                if (count > 0)
                    items.Add(new PluginLoadItem(Path.GetFileNameWithoutExtension(dllPath), Path.GetFileNameWithoutExtension(dllPath), "legacy", true, true, null));
            }
            catch (Exception ex)
            {
                failedPlugins++;
                errors.Add($"Load failed: {Path.GetFileName(dllPath)} => {ex.Message}");
                items.Add(new PluginLoadItem(Path.GetFileNameWithoutExtension(dllPath), Path.GetFileNameWithoutExtension(dllPath), "legacy", true, false, ex.Message));
            }
        }

        return new PluginLoadSummary(pluginsDirectory, loadedPlugins, failedPlugins, errors, items);
    }

    private static int RegisterFromAssembly(ShellContext shell, Assembly assembly, string sourcePath, List<string> errors)
    {
        var registeredCount = 0;
        var exportedTypes = Array.Empty<Type>();

        try
        {
            exportedTypes = assembly.GetExportedTypes();
        }
        catch (Exception ex)
        {
            errors.Add($"Type discovery failed: {Path.GetFileName(sourcePath)} => {ex.Message}");
            return 0;
        }

        foreach (var type in exportedTypes)
        {
            var registeredViaIPlugin = false;

            // Contract-based plugin
            if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            {
                try
                {
                    if (Activator.CreateInstance(type) is IPlugin plugin)
                    {
                        plugin.Register(shell);
                        registeredCount++;
                        registeredViaIPlugin = true;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"IPlugin register failed: {type.FullName} => {ex.Message}");
                }
            }

            if (registeredViaIPlugin)
                continue;

            // Convention-based plugin entrypoint
            var registerMethod = type.GetMethod(
                "Register",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: [typeof(ShellContext)],
                modifiers: null);

            if (registerMethod == null || registerMethod.ReturnType != typeof(void))
                continue;

            try
            {
                registerMethod.Invoke(null, [shell]);
                registeredCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"Static Register failed: {type.FullName} => {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        return registeredCount;
    }
}

internal sealed record PluginLoadOptions(
    string HostVersion = "0.1.0-preview",
    HashSet<string>? DisabledPluginIds = null)
{
    public HashSet<string> DisabledPluginIds { get; } = DisabledPluginIds ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}

internal sealed record PluginLoadSummary(
    string Directory,
    int LoadedPlugins,
    int FailedPlugins,
    IReadOnlyList<string> Errors,
    IReadOnlyList<PluginLoadItem> Items);

internal sealed record PluginLoadItem(
    string Id,
    string Name,
    string Version,
    bool Enabled,
    bool Loaded,
    string? Error);
