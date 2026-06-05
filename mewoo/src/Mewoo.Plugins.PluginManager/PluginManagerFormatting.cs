using Mewoo.Core.Plugins;

namespace Mewoo.Plugins.PluginManager;

internal static class PluginManagerFormatting
{
    public static string EntryKey(MewooPluginManagerCatalogEntry entry) =>
        entry.PluginId ?? entry.PluginDirectory;

    public static string FilterLabel(MewooPluginManagerCatalogFilter filter) =>
        filter switch
        {
            MewooPluginManagerCatalogFilter.All => "All",
            MewooPluginManagerCatalogFilter.Enabled => "Enabled",
            MewooPluginManagerCatalogFilter.Disabled => "Disabled",
            MewooPluginManagerCatalogFilter.Failed => "Failed",
            MewooPluginManagerCatalogFilter.Incompatible => "Incompatible",
            MewooPluginManagerCatalogFilter.Broken => "Broken",
            _ => filter.ToString(),
        };

    public static string FormatPublisher(MewooPluginManagerCatalogEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.PublisherDisplayName) && !string.IsNullOrWhiteSpace(entry.Publisher))
        {
            return $"{entry.PublisherDisplayName} ({entry.Publisher})";
        }

        return entry.PublisherDisplayName ?? entry.Publisher ?? "Not declared";
    }

    public static string FormatPublisher(MewooPluginInstallPreview preview)
    {
        if (!string.IsNullOrWhiteSpace(preview.PublisherDisplayName) && !string.IsNullOrWhiteSpace(preview.Publisher))
        {
            return $"{preview.PublisherDisplayName} ({preview.Publisher})";
        }

        return preview.PublisherDisplayName ?? preview.Publisher ?? "Not declared";
    }

    public static string FormatPublisher(MewooPluginUpdatePreview preview)
    {
        if (!string.IsNullOrWhiteSpace(preview.PublisherDisplayName) && !string.IsNullOrWhiteSpace(preview.Publisher))
        {
            return $"{preview.PublisherDisplayName} ({preview.Publisher})";
        }

        return preview.PublisherDisplayName ?? preview.Publisher ?? "Not declared";
    }
}
