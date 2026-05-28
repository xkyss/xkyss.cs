namespace Mewoo.Core.Plugins;

public sealed record MewooRuntimePluginStatus(
    MewooRuntimePluginDescriptor Descriptor,
    MewooRuntimePluginState State,
    string? Message = null,
    MewooRuntimePluginIssueCategory IssueCategory = MewooRuntimePluginIssueCategory.None)
{
    public string CategoryLabel => IssueCategory switch
    {
        MewooRuntimePluginIssueCategory.Manifest => "Manifest",
        MewooRuntimePluginIssueCategory.Compatibility => "Compatibility",
        MewooRuntimePluginIssueCategory.Assembly => "Assembly",
        MewooRuntimePluginIssueCategory.EntryPoint => "Entry point",
        MewooRuntimePluginIssueCategory.Registration => "Registration",
        MewooRuntimePluginIssueCategory.Activation => "Activation",
        MewooRuntimePluginIssueCategory.Disabled => "Disabled",
        MewooRuntimePluginIssueCategory.Unload => "Unload",
        MewooRuntimePluginIssueCategory.Package => "Package",
        _ => State switch
        {
            MewooRuntimePluginState.Disabled => "Disabled",
            MewooRuntimePluginState.Incompatible => "Compatibility",
            MewooRuntimePluginState.Failed => "Runtime failure",
            _ => "Runtime",
        },
    };

    public string ShortMessage
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Message))
            {
                return State switch
                {
                    MewooRuntimePluginState.Discovered => "Plugin manifest was discovered.",
                    MewooRuntimePluginState.Disabled => "Plugin is disabled by manifest.",
                    MewooRuntimePluginState.Incompatible => "Plugin is not compatible with this Mewoo version.",
                    MewooRuntimePluginState.Loaded => "Plugin assembly was loaded.",
                    MewooRuntimePluginState.Registered => "Plugin is registered.",
                    MewooRuntimePluginState.Failed => "Plugin failed.",
                    _ => "Plugin status is unknown.",
                };
            }

            var trimmed = Message.Trim();
            var lineEnd = trimmed.IndexOfAny(['\r', '\n']);
            return lineEnd < 0 ? trimmed : trimmed[..lineEnd];
        }
    }
}

public enum MewooRuntimePluginState
{
    Discovered,
    Disabled,
    Incompatible,
    Loaded,
    Registered,
    Failed,
}
