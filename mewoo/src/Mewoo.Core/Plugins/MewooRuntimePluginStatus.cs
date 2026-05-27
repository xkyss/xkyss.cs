namespace Mewoo.Core.Plugins;

public sealed record MewooRuntimePluginStatus(
    MewooRuntimePluginDescriptor Descriptor,
    MewooRuntimePluginState State,
    string? Message = null);

public enum MewooRuntimePluginState
{
    Discovered,
    Disabled,
    Incompatible,
    Loaded,
    Registered,
    Failed,
}

