using Mewoo.Core.Logging;
using Mewoo.Core.Plugins;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class MewooRuntimePluginCatalogTests
{
    [TestMethod]
    public void DiscoverSkipsDisabledPlugins()
    {
        using var directory = TestPluginRoot.Create();
        directory.WriteManifest("disabledPlugin",
            """
            {
              "id": "xkyss.disabledPlugin",
              "displayName": "Disabled Plugin",
              "version": "0.1.0",
              "assembly": "DisabledPlugin.dll",
              "entryPoint": "Plugin",
              "disabled": true
            }
            """);

        var logger = new InMemoryMewooLogger();
        var descriptors = new MewooRuntimePluginCatalog(logger).Discover(directory.Root);

        Assert.AreEqual(0, descriptors.Count);
        Assert.IsTrue(logger.Entries.Any(entry => entry.Message.Contains("Skipped disabled runtime plugin", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void DiscoverLogsInvalidManifestAndContinues()
    {
        using var directory = TestPluginRoot.Create();
        directory.WriteManifest("invalidPlugin",
            """
            {
              "id": "bad-id",
              "displayName": "Invalid Plugin",
              "version": "0.1.0",
              "assembly": "InvalidPlugin.dll",
              "entryPoint": "Plugin"
            }
            """);
        directory.WriteManifest("validPlugin",
            """
            {
              "id": "xkyss.validPlugin",
              "displayName": "Valid Plugin",
              "version": "0.1.0",
              "assembly": "ValidPlugin.dll",
              "entryPoint": "Plugin"
            }
            """);

        var logger = new InMemoryMewooLogger();
        var descriptors = new MewooRuntimePluginCatalog(logger).Discover(directory.Root);

        Assert.AreEqual(1, descriptors.Count);
        Assert.AreEqual("xkyss.validPlugin", descriptors[0].Manifest.Id);
        Assert.IsTrue(logger.Entries.Any(entry => entry.Message.Contains("Failed to read runtime plugin manifest", StringComparison.Ordinal)));
    }

    private sealed class TestPluginRoot : IDisposable
    {
        private TestPluginRoot(string root)
        {
            Root = root;
            Directory.CreateDirectory(root);
        }

        public string Root { get; }

        public static TestPluginRoot Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "Mewoo.Core.Tests", Guid.NewGuid().ToString("N"));
            return new TestPluginRoot(root);
        }

        public void WriteManifest(string directoryName, string json)
        {
            var pluginDirectory = Path.Combine(Root, directoryName);
            Directory.CreateDirectory(pluginDirectory);
            File.WriteAllText(Path.Combine(pluginDirectory, MewooRuntimePluginCatalog.ManifestFileName), json);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
    }
}

