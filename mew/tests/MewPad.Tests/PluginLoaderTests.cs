using MewPad.Core.Shell;
using MewPad.Hosting.Plugins;
using QuickLaunch.Plugin;

namespace MewPad.Tests;

public class PluginLoaderTests
{
    [Fact]
    public void LoadFromDirectory_LoadsManifestPluginSuccessfully()
    {
        // Arrange
        var shell = new ShellContext();
        var tempDir = Path.Combine(Path.GetTempPath(), "mewpad-plugin-test-" + Guid.NewGuid());
        var pluginDir = Path.Combine(tempDir, "QuickLaunch");
        Directory.CreateDirectory(pluginDir);

        var pluginAssemblyPath = typeof(QuickLaunchPlugin).Assembly.Location;
        var targetAssemblyPath = Path.Combine(pluginDir, "QuickLaunch.Plugin.dll");
        File.Copy(pluginAssemblyPath, targetAssemblyPath, true);

        File.WriteAllText(Path.Combine(pluginDir, "plugin.json"), """
{
  "id": "quicklaunch.plugin",
  "name": "QuickLaunch",
  "version": "0.1.0",
  "minHostVersion": "0.1.0",
  "entryAssembly": "QuickLaunch.Plugin.dll"
}
""");

        // Act
        var summary = PluginLoader.LoadFromDirectory(shell, tempDir, new PluginLoadOptions("0.1.0-preview"));

        // Assert
        Assert.True(summary.LoadedPlugins >= 1);
        Assert.Contains(shell.GetActivities(), a => a.Id == "quicklaunch.home");

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void LoadFromDirectory_RespectsDisabledPlugins()
    {
        // Arrange
        var shell = new ShellContext();
        var tempDir = Path.Combine(Path.GetTempPath(), "mewpad-plugin-test-" + Guid.NewGuid());
        var pluginDir = Path.Combine(tempDir, "QuickLaunch");
        Directory.CreateDirectory(pluginDir);

        var pluginAssemblyPath = typeof(QuickLaunchPlugin).Assembly.Location;
        File.Copy(pluginAssemblyPath, Path.Combine(pluginDir, "QuickLaunch.Plugin.dll"), true);

        File.WriteAllText(Path.Combine(pluginDir, "plugin.json"), """
{
  "id": "quicklaunch.plugin",
  "name": "QuickLaunch",
  "version": "0.1.0",
  "minHostVersion": "0.1.0",
  "entryAssembly": "QuickLaunch.Plugin.dll"
}
""");

        var disabled = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "quicklaunch.plugin" };

        // Act
        var summary = PluginLoader.LoadFromDirectory(shell, tempDir, new PluginLoadOptions("0.1.0-preview", disabled));

        // Assert
        Assert.Equal(0, summary.LoadedPlugins);
        Assert.DoesNotContain(shell.GetActivities(), a => a.Id == "quicklaunch.home");

        // Cleanup
        Directory.Delete(tempDir, true);
    }
}
