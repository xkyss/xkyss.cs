using Mewoo.Core.Plugins;

namespace Mewoo.Plugins.PluginManager;

internal sealed class PluginManagerSession
{
    public string SearchText { get; set; } = string.Empty;

    public MewooPluginManagerCatalogFilter Filter { get; set; } = MewooPluginManagerCatalogFilter.All;

    public string? SelectedEntryKey { get; set; }

    public MewooPluginInstallPreview? InstallPreview { get; set; }

    public MewooPluginUpdatePreview? UpdatePreview { get; set; }

    public List<string> OperationResults { get; } = [];

    public void AddOperation(string message)
    {
        OperationResults.Insert(0, $"[{DateTimeOffset.Now:HH:mm:ss}] {message}");
        if (OperationResults.Count > 25)
        {
            OperationResults.RemoveRange(25, OperationResults.Count - 25);
        }
    }
}
