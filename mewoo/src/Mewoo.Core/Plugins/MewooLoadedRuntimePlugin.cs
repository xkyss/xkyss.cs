using Mewoo.Abstractions.Plugins;

namespace Mewoo.Core.Plugins;

public sealed record MewooLoadedRuntimePlugin(
    MewooRuntimePluginDescriptor Descriptor,
    IMewooPlugin Plugin,
    MewooRuntimePluginLoadContext LoadContext);

