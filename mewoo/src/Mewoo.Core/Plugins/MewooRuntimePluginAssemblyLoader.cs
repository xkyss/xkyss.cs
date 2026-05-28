using Mewoo.Abstractions.Logging;

namespace Mewoo.Core.Plugins;

public sealed class MewooRuntimePluginAssemblyLoader
{
    private readonly IMewooLogger? _logger;

    public MewooRuntimePluginAssemblyLoader(IMewooLogger? logger = null)
    {
        _logger = logger;
    }

    public MewooLoadedRuntimePluginAssembly? TryLoad(
        MewooRuntimePluginDescriptor descriptor,
        out MewooRuntimePluginIssue? issue)
    {
        issue = null;
        try
        {
            var loadContext = new MewooRuntimePluginLoadContext(descriptor.AssemblyPath);
            var assembly = loadContext.LoadFromAssemblyPath(descriptor.AssemblyPath);
            _logger?.Info("RuntimePluginLoader", $"Loaded runtime plugin assembly '{descriptor.Manifest.Id}'.");

            return new MewooLoadedRuntimePluginAssembly(descriptor, assembly, loadContext);
        }
        catch (Exception ex)
        {
            issue = new MewooRuntimePluginIssue(
                MewooRuntimePluginIssueCategory.Assembly,
                ex.Message,
                descriptor.ManifestPath,
                descriptor.AssemblyPath);
            _logger?.Error(
                "RuntimePluginLoader",
                $"Failed to load runtime plugin assembly '{descriptor.Manifest.Id}'.",
                ex);
            return null;
        }
    }
}
