using Mewoo.Abstractions.Contributions;
using Mewoo.Core.Plugins;
using Mewoo.Plugins.PluginManager;
using Mewoo.Plugins.Settings;
using Mewoo.Workbench;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class MewooBuiltInSystemActivityTests
{
    [TestMethod]
    public void PluginManagerContributesSystemActivity()
    {
        var host = new MewooPluginHost();
        using var pluginRoot = new TestPluginRoot();
        var plugin = new PluginManagerPlugin(
            new MewooRuntimePluginManager(),
            host,
            EmptyServiceProvider.Instance,
            pluginRoot.Path);

        var entry = host.RegisterPlugin(plugin, MewooPluginRegistrationSource.BuiltIn);

        Assert.AreEqual(Mewoo.Abstractions.Plugins.MewooPluginState.Registered, entry.State);
        var activity = host.GetRegisteredContributions("pluginManager").Activities.Single(item =>
            item.Id == "pluginManager.activity");
        Assert.AreEqual(ActivityBarSection.System, activity.Section);
        Assert.AreEqual(80, activity.Order);
    }

    [TestMethod]
    public void SettingsContributesBottomSystemActivity()
    {
        var host = new MewooPluginHost();

        var entry = host.RegisterPlugin(new SettingsPlugin(), MewooPluginRegistrationSource.BuiltIn);

        Assert.AreEqual(Mewoo.Abstractions.Plugins.MewooPluginState.Registered, entry.State);
        var activity = host.GetRegisteredContributions("settings").Activities.Single(item =>
            item.Id == "settings.activity");
        Assert.AreEqual(ActivityBarSection.System, activity.Section);
        Assert.AreEqual(90, activity.Order);
        Assert.IsTrue(host.GetRegisteredContributions("settings").MainViews.Any(item =>
            item.Id == "settings.home"));
        Assert.IsTrue(host.GetRegisteredContributions("settings").Commands.Any(item =>
            item.Id == "settings.open"));
    }

    [TestMethod]
    public void PluginManagerRendersAboveSettingsInSystemSection()
    {
        var host = new MewooPluginHost();
        using var pluginRoot = new TestPluginRoot();
        host.RegisterPlugin(
            new PluginManagerPlugin(new MewooRuntimePluginManager(), host, EmptyServiceProvider.Instance, pluginRoot.Path),
            MewooPluginRegistrationSource.BuiltIn);
        host.RegisterPlugin(new SettingsPlugin(), MewooPluginRegistrationSource.BuiltIn);
        var pluginManagerSnapshot = host.GetRegisteredContributions("pluginManager");
        var settingsSnapshot = host.GetRegisteredContributions("settings");

        var sections = WorkbenchActivityBarModel.CreateSections(
            pluginManagerSnapshot.Activities.Concat(settingsSnapshot.Activities));

        CollectionAssert.AreEqual(
            new[] { "pluginManager.activity", "settings.activity" },
            sections.System.Select(activity => activity.Id).ToArray());
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public static readonly EmptyServiceProvider Instance = new();

        public object? GetService(Type serviceType) => null;
    }

    private sealed class TestPluginRoot : IDisposable
    {
        public TestPluginRoot()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "Mewoo.Core.Tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
