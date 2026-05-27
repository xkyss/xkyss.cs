using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Plugins;

namespace Mewoo.TestPlugins.ValidRuntimePlugin;

public sealed class ValidRuntimePlugin : IMewooPlugin
{
    public const string PluginId = "xkyss.validRuntimePlugin";

    public string Id => PluginId;

    public string DisplayName => "Valid Runtime Plugin";

    public void Register(IMewooContributionRegistry registry)
    {
    }
}

public sealed class DifferentIdRuntimePlugin : IMewooPlugin
{
    public string Id => "xkyss.differentRuntimePlugin";

    public string DisplayName => "Different Runtime Plugin";

    public void Register(IMewooContributionRegistry registry)
    {
    }
}

public sealed class ThrowingRuntimePlugin : IMewooPlugin
{
    public ThrowingRuntimePlugin()
    {
        throw new InvalidOperationException("Constructor failed.");
    }

    public string Id => "xkyss.throwingRuntimePlugin";

    public string DisplayName => "Throwing Runtime Plugin";

    public void Register(IMewooContributionRegistry registry)
    {
    }
}

public sealed class NotAPlugin
{
}

