namespace Mewoo.Core.Plugins;

public sealed record MewooPluginOperationResult(
    bool Success,
    string? PluginId,
    string? Path,
    MewooRuntimePluginIssue? Issue)
{
    public static MewooPluginOperationResult Succeeded(string pluginId, string? path = null) =>
        new(true, pluginId, path, null);

    public static MewooPluginOperationResult Failed(
        string? pluginId,
        MewooRuntimePluginIssue issue,
        string? path = null) =>
        new(false, pluginId, path, issue);
}

