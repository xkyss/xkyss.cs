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

            Directory.Delete(installPath, recursive: true);
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
}

