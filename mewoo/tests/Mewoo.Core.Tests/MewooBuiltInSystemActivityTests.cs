using Mewoo.Abstractions.Contributions;
using Mewoo.Core.Plugins;
using Mewoo.Plugins.PluginManager;

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
