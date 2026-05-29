using Mewoo.Abstractions.Plugins;

namespace Mewoo.Core.Plugins;

public sealed class MewooPluginPackageOperations
{
    private readonly MewooPluginInstaller _installer;
    private readonly MewooPluginPackageReader _packageReader;

    public MewooPluginPackageOperations(
        MewooPluginInstaller? installer = null,
        MewooPluginPackageReader? packageReader = null)
    {
        _installer = installer ?? new MewooPluginInstaller();
        _packageReader = packageReader ?? new MewooPluginPackageReader();
    }

    public async ValueTask<MewooPluginOperationResult> UninstallAsync(
        string pluginId,
        string pluginRoot,
        MewooRuntimePluginManager runtimePlugins,
        MewooPluginHost pluginHost,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
        {
            throw new ArgumentException("Plugin id is required.", nameof(pluginId));
        }

        if (string.IsNullOrWhiteSpace(pluginRoot))
        {
            throw new ArgumentException("Plugin root is required.", nameof(pluginRoot));
        }

        var installPath = Path.Combine(Path.GetFullPath(pluginRoot), pluginId);
        try
        {
            await runtimePlugins.UnloadPluginAsync(pluginId, pluginHost, cancellationToken);
            if (!Directory.Exists(installPath))
            {
                return MewooPluginOperationResult.Failed(
                    pluginId,
                    new MewooRuntimePluginIssue(
                        MewooRuntimePluginIssueCategory.Package,
                        $"Installed plugin directory was not found: {installPath}",
                        installPath),
                    installPath);
            }

            await DeleteDirectoryWithRetryAsync(installPath, cancellationToken);
            return MewooPluginOperationResult.Succeeded(pluginId, installPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return MewooPluginOperationResult.Failed(
                pluginId,
                new MewooRuntimePluginIssue(
                    MewooRuntimePluginIssueCategory.Package,
                    ex.Message,
                    installPath),
                installPath);
        }
    }

    public async ValueTask<MewooPluginOperationResult> RemoveBrokenInstallAsync(
        MewooPluginManagerCatalogEntry entry,
        string pluginRoot,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pluginRoot))
        {
            throw new ArgumentException("Plugin root is required.", nameof(pluginRoot));
        }

        var fullPluginRoot = Path.GetFullPath(pluginRoot);
        var installPath = Path.GetFullPath(entry.PluginDirectory);
        if (entry.State != MewooPluginManagerCatalogState.Broken)
        {
            return MewooPluginOperationResult.Failed(
                entry.PluginId,
                new MewooRuntimePluginIssue(
                    MewooRuntimePluginIssueCategory.Package,
                    "Only broken plugin installs can be removed with this operation.",
                    installPath),
                installPath);
        }

        if (!IsDirectoryInsidePluginRoot(installPath, fullPluginRoot))
        {
            return MewooPluginOperationResult.Failed(
                entry.PluginId,
                new MewooRuntimePluginIssue(
                    MewooRuntimePluginIssueCategory.Package,
                    "Broken plugin directory is outside the configured plugin root.",
                    installPath),
                installPath);
        }

        if (!Directory.Exists(installPath))
        {
            return MewooPluginOperationResult.Failed(
                entry.PluginId,
                new MewooRuntimePluginIssue(
                    MewooRuntimePluginIssueCategory.Package,
                    $"Broken plugin directory was not found: {installPath}",
                    installPath),
                installPath);
        }

        try
        {
            await DeleteDirectoryWithRetryAsync(installPath, cancellationToken);
            return MewooPluginOperationResult.Succeeded(entry.PluginId ?? entry.DisplayName, installPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return MewooPluginOperationResult.Failed(
                entry.PluginId,
                new MewooRuntimePluginIssue(
                    MewooRuntimePluginIssueCategory.Package,
                    ex.Message,
                    installPath),
                installPath);
        }
    }

    public async ValueTask<MewooPluginOperationResult> UpdateAsync(
        string packagePath,
        string pluginRoot,
        MewooRuntimePluginManager runtimePlugins,
        MewooPluginHost pluginHost,
        Func<IMewooPlugin, IMewooPluginContext> createContext,
        CancellationToken cancellationToken = default)
    {
        var packageResult = _packageReader.Read(packagePath);
        if (!packageResult.Success || packageResult.Package is null)
        {
            return MewooPluginOperationResult.Failed(null, packageResult.Issue!, packagePath);
        }

        var pluginId = packageResult.Package.PackageId;
        var wasLoaded = runtimePlugins.LoadedPlugins.Any(plugin =>
            string.Equals(plugin.Plugin.Id, pluginId, StringComparison.Ordinal));

        if (wasLoaded)
        {
            await runtimePlugins.UnloadPluginAsync(pluginId, pluginHost, cancellationToken);
        }

        var install = _installer.Install(packagePath, pluginRoot, expectedPluginId: pluginId);
        if (!install.Success)
        {
            if (wasLoaded)
            {
                await runtimePlugins.ReloadPluginAsync(pluginId, pluginRoot, pluginHost, createContext, cancellationToken);
            }

            return MewooPluginOperationResult.Failed(pluginId, install.Issue!, install.InstalledPath);
        }

        var reloaded = await runtimePlugins.ReloadPluginAsync(pluginId, pluginRoot, pluginHost, createContext, cancellationToken);
        if (!reloaded)
        {
            return MewooPluginOperationResult.Failed(
                pluginId,
                new MewooRuntimePluginIssue(
                    MewooRuntimePluginIssueCategory.Package,
                    $"Plugin package '{pluginId}' was installed but could not be loaded.",
                    packagePath),
                install.InstalledPath);
        }

        return MewooPluginOperationResult.Succeeded(pluginId, install.InstalledPath);
    }

    private static async ValueTask DeleteDirectoryWithRetryAsync(string path, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 8; attempt++)
        {
            try
            {
                Directory.Delete(path, recursive: true);
                return;
            }
            catch (Exception ex) when (attempt < 7 && ex is IOException or UnauthorizedAccessException)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                await Task.Delay(50, cancellationToken);
            }
        }

        Directory.Delete(path, recursive: true);
    }

    private static bool IsDirectoryInsidePluginRoot(string directory, string pluginRoot)
    {
        var relativePath = Path.GetRelativePath(pluginRoot, directory);
        return !string.IsNullOrWhiteSpace(relativePath)
            && relativePath != "."
            && relativePath != ".."
            && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            && !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
            && !Path.IsPathRooted(relativePath);
    }
}
