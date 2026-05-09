namespace MewPad.Hosting.Plugins;

using MewPad.Core.Services;

internal static class PluginConfig
{
    private const string DisabledPluginsKey = "plugins.disabledIds";

    public static HashSet<string> GetDisabledPluginIds(IConfigurationService configuration)
    {
        var ids = configuration.GetConfig<string[]>(DisabledPluginsKey, []);
        return ids == null
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
    }

    public static void SetPluginEnabled(IConfigurationService configuration, string pluginId, bool enabled)
    {
        var disabled = GetDisabledPluginIds(configuration);
        if (enabled)
            disabled.Remove(pluginId);
        else
            disabled.Add(pluginId);

        configuration.SetConfig(DisabledPluginsKey, disabled.OrderBy(x => x).ToArray());
        configuration.Save();
    }
}
