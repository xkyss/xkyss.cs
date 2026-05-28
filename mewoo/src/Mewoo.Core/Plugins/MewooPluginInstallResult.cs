namespace Mewoo.Core.Plugins;

public sealed record MewooPluginInstallResult(
    bool Success,
    string? PluginId,
    string? InstalledPath,
    MewooRuntimePluginIssue? Issue)
{
    public static MewooPluginInstallResult Succeeded(string pluginId, string installedPath) =>
        new(true, pluginId, installedPath, null);

    public static MewooPluginInstallResult Failed(MewooRuntimePluginIssue issue, string? pluginId = null) =>
        new(false, pluginId, null, issue);
}

