using Mewoo.Core.Plugins;

namespace Mewoo.Plugins.PluginManager;

internal static class PluginManagerFormatting
{
    public static string EntryKey(MewooPluginManagerCatalogEntry entry) =>
        entry.PluginId ?? entry.PluginDirectory;

    public static string CategoryLabel(PluginManagerCategory category) =>
        category switch
        {
            PluginManagerCategory.All => "All",
            PluginManagerCategory.Enabled => "Enabled",
            PluginManagerCategory.Disabled => "Disabled",
            PluginManagerCategory.NeedsAttention => "Needs Attention",
            _ => category.ToString(),
        };

    public static string CategoryId(PluginManagerCategory category) =>
        category switch
        {
            PluginManagerCategory.All => "category.all",
            PluginManagerCategory.Enabled => "category.enabled",
            PluginManagerCategory.Disabled => "category.disabled",
            PluginManagerCategory.NeedsAttention => "category.needsAttention",
            _ => $"category.{category}",
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
