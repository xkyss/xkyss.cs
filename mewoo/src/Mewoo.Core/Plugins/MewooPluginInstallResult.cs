namespace Mewoo.Core.Plugins;

public sealed record MewooPluginInstallResult(
    bool Success,
    string? PluginId,
    string? InstalledPath,
    MewooRuntimePluginIssue? Issue,
    string? TrustWarning = null)
{
    public static MewooPluginInstallResult Succeeded(
        string pluginId,
        string installedPath,
        string? trustWarning = null) =>
        new(true, pluginId, installedPath, null, trustWarning);

    public static MewooPluginInstallResult Failed(MewooRuntimePluginIssue issue, string? pluginId = null) =>
        new(false, pluginId, null, issue);
}
