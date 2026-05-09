namespace MewPad.Hosting.Plugins;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewPad.Core.Interfaces;
using MewPad.Core.Services;

internal sealed class PluginManagementSettings(IConfigurationService configuration, PluginLoadSummary summary) : ISettingsCategory
{
    public string Id => "plugins";
    public string Title => "Plugins";
    public object? Icon => "🧩";
    public int Order => 80;

    public FrameworkElement CreateView()
    {
        var root = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(8) };
        root.Children(new Label { Text = "Plugin Management", FontSize = 16, FontWeight = FontWeight.SemiBold });
        root.Children(new Label { Text = "Changes take effect after restart.", FontSize = 11, Margin = new Thickness(0, 6, 0, 8) });

        if (summary.Items.Count == 0)
        {
            root.Children(new Label { Text = "No plugin manifests found.", FontSize = 11 });
            return root;
        }

        foreach (var item in summary.Items.OrderBy(x => x.Id, StringComparer.OrdinalIgnoreCase))
        {
            var pluginId = item.Id;
            var statusLabel = new Label { Text = BuildStatusText(item), FontSize = 11, Margin = new Thickness(0, 2, 0, 4) };
            var toggleButton = new Button
            {
                Content = new Label { Text = item.Enabled ? "Disable" : "Enable" },
                MinWidth = 90,
            };

            toggleButton.OnClick(() =>
            {
                PluginConfig.SetPluginEnabled(configuration, pluginId, enabled: !item.Enabled);
                statusLabel.Text = "Saved. Restart MewPad to apply.";
            });

            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            row.Children(new Label { Text = $"{item.Name} ({item.Version})", FontSize = 12, Margin = new Thickness(0, 0, 12, 0) });
            row.Children(toggleButton);

            root.Children(row);
            root.Children(statusLabel);
        }

        return root;
    }

    private static string BuildStatusText(PluginLoadItem item)
    {
        if (!item.Enabled)
            return "Disabled by configuration";
        if (!string.IsNullOrWhiteSpace(item.Error))
            return $"Error: {item.Error}";
        return item.Loaded ? "Loaded" : "Not loaded";
    }
}
