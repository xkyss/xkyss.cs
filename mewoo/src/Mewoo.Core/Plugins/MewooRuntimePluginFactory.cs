using Mewoo.Abstractions.Logging;
using Mewoo.Abstractions.Plugins;

namespace Mewoo.Core.Plugins;

public sealed class MewooRuntimePluginFactory
{
    private readonly MewooRuntimePluginAssemblyLoader _assemblyLoader;
    private readonly IMewooLogger? _logger;

    public MewooRuntimePluginFactory(IMewooLogger? logger = null, MewooRuntimePluginAssemblyLoader? assemblyLoader = null)
    {
        _logger = logger;
        _assemblyLoader = assemblyLoader ?? new MewooRuntimePluginAssemblyLoader(logger);
    }

    public MewooLoadedRuntimePlugin? TryCreate(MewooRuntimePluginDescriptor descriptor)
    {
        var loadedAssembly = _assemblyLoader.TryLoad(descriptor);
        if (loadedAssembly is null)
        {
            return null;
        }

        try
        {
            var entryPointType = loadedAssembly.Assembly.GetType(descriptor.Manifest.EntryPoint, throwOnError: false);
            if (entryPointType is null)
            {
                throw new InvalidOperationException($"Runtime plugin entry point '{descriptor.Manifest.EntryPoint}' was not found.");
            }

            if (!typeof(IMewooPlugin).IsAssignableFrom(entryPointType))
            {
                throw new InvalidOperationException($"Runtime plugin entry point '{descriptor.Manifest.EntryPoint}' does not implement IMewooPlugin.");
            }

            var plugin = Activator.CreateInstance(entryPointType) as IMewooPlugin
                ?? throw new InvalidOperationException($"Runtime plugin entry point '{descriptor.Manifest.EntryPoint}' could not be created.");

            if (!string.Equals(plugin.Id, descriptor.Manifest.Id, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Runtime plugin manifest id '{descriptor.Manifest.Id}' does not match plugin id '{plugin.Id}'.");
            }

            _logger?.Info("RuntimePluginFactory", $"Created runtime plugin '{plugin.Id}'.");
            return new MewooLoadedRuntimePlugin(descriptor, plugin, loadedAssembly.LoadContext);
        }
        catch (Exception ex)
        {
            loadedAssembly.LoadContext.Unload();
            _logger?.Error("RuntimePluginFactory", $"Failed to create runtime plugin '{descriptor.Manifest.Id}'.", ex);
            return null;
        }
    }
}

