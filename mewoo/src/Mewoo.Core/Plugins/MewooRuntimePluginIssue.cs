namespace Mewoo.Core.Plugins;

public sealed record MewooRuntimePluginIssue(
    MewooRuntimePluginIssueCategory Category,
    string Message,
    string ManifestPath,
    string? AssemblyPath = null)
{
    public string ShortMessage => FirstLine(Message);

    private static string FirstLine(string message)
    {
        var trimmed = message.Trim();
        var lineEnd = trimmed.IndexOfAny(['\r', '\n']);
        return lineEnd < 0 ? trimmed : trimmed[..lineEnd];
    }
}

public enum MewooRuntimePluginIssueCategory
{
    None,
    Manifest,
    Compatibility,
    Assembly,
    EntryPoint,
    Registration,
    Activation,
    Disabled,
    Unload,
}
