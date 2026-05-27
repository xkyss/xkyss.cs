using System.Reflection;
using System.Runtime.Loader;

namespace Mewoo.Core.Plugins;

public sealed class MewooRuntimePluginLoadContext : AssemblyLoadContext
{
    private static readonly string[] SharedAssemblyNamePrefixes =
    [
        "Mewoo.Abstractions",
        "Aprillz.MewUI",
    ];

    private readonly AssemblyDependencyResolver _resolver;

    public MewooRuntimePluginLoadContext(string mainAssemblyPath)
        : base(isCollectible: true)
    {
        if (string.IsNullOrWhiteSpace(mainAssemblyPath))
        {
            throw new ArgumentException("Main assembly path is required.", nameof(mainAssemblyPath));
        }

        _resolver = new AssemblyDependencyResolver(mainAssemblyPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        return ResolveAssembly(assemblyName);
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return libraryPath is null
            ? IntPtr.Zero
            : LoadUnmanagedDllFromPath(libraryPath);
    }

    internal Assembly? ResolveAssembly(AssemblyName assemblyName)
    {
        if (IsSharedAssembly(assemblyName))
        {
            return AssemblyLoadContext.Default.Assemblies.FirstOrDefault(assembly =>
                AssemblyName.ReferenceMatchesDefinition(assembly.GetName(), assemblyName));
        }

        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        return assemblyPath is null ? null : LoadFromAssemblyPath(assemblyPath);
    }

    private static bool IsSharedAssembly(AssemblyName assemblyName)
    {
        if (assemblyName.Name is null)
        {
            return false;
        }

        return SharedAssemblyNamePrefixes.Any(prefix =>
            assemblyName.Name.Equals(prefix, StringComparison.Ordinal)
            || assemblyName.Name.StartsWith(prefix + ".", StringComparison.Ordinal));
    }
}

