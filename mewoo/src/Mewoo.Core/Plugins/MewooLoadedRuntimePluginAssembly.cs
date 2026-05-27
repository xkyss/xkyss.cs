using System.Reflection;

namespace Mewoo.Core.Plugins;

public sealed record MewooLoadedRuntimePluginAssembly(
    MewooRuntimePluginDescriptor Descriptor,
    Assembly Assembly,
    MewooRuntimePluginLoadContext LoadContext);

