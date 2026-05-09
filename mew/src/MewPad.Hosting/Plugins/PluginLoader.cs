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
    public static PluginLoadSummary LoadFromDirectory(ShellContext shell, string pluginsDirectory)
    {
        if (!Directory.Exists(pluginsDirectory))
            return new PluginLoadSummary(pluginsDirectory, 0, 0, []);

        var dllFiles = Directory.GetFiles(pluginsDirectory, "*.dll", SearchOption.TopDirectoryOnly);
        var loadedPlugins = 0;
        var failedPlugins = 0;
        var errors = new List<string>();

        foreach (var dllPath in dllFiles)
        {
            try
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(dllPath));
                var count = RegisterFromAssembly(shell, assembly, dllPath, errors);
                loadedPlugins += count;
            }
            catch (Exception ex)
            {
                failedPlugins++;
                errors.Add($"Load failed: {Path.GetFileName(dllPath)} => {ex.Message}");
            }
        }

        return new PluginLoadSummary(pluginsDirectory, loadedPlugins, failedPlugins, errors);
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

internal sealed record PluginLoadSummary(
    string Directory,
    int LoadedPlugins,
    int FailedPlugins,
    IReadOnlyList<string> Errors);
