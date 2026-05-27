using Mewoo.Core.Plugins;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class MewooPluginManifestReaderTests
{
    [TestMethod]
    public void ReadValidManifestReturnsDescriptor()
    {
        using var directory = TestPluginDirectory.Create();
        var manifestPath = directory.WriteManifest(
            """
            {
              "id": "xkyss.quickLauncherPlus",
              "displayName": "Quick Launcher Plus",
              "version": "0.1.0",
              "assembly": "QuickLauncherPlus.dll",
              "entryPoint": "Xkyss.Mewoo.Plugins.QuickLauncherPlus.Plugin",
              "minimumMewooVersion": "1.0.0",
              "metadata": {
                "author": "xkyss"
              }
            }
            """);

        var descriptor = new MewooPluginManifestReader().Read(manifestPath);

        Assert.AreEqual("xkyss.quickLauncherPlus", descriptor.Manifest.Id);
        Assert.AreEqual("Quick Launcher Plus", descriptor.Manifest.DisplayName);
        Assert.AreEqual("0.1.0", descriptor.Manifest.Version);
        Assert.AreEqual("Xkyss.Mewoo.Plugins.QuickLauncherPlus.Plugin", descriptor.Manifest.EntryPoint);
        Assert.AreEqual("xkyss", descriptor.Manifest.Metadata["author"]);
        Assert.AreEqual(Path.Combine(directory.PluginDirectory, "QuickLauncherPlus.dll"), descriptor.AssemblyPath);
    }

    [TestMethod]
    public void MissingRequiredFieldFails()
    {
        using var directory = TestPluginDirectory.Create();
        var manifestPath = directory.WriteManifest(
            """
            {
              "id": "xkyss.quickLauncherPlus",
              "displayName": "Quick Launcher Plus",
              "version": "0.1.0",
              "assembly": "QuickLauncherPlus.dll"
            }
            """);

        var ex = ThrowsInvalidOperation(() => new MewooPluginManifestReader().Read(manifestPath));

        StringAssert.Contains(ex.Message, "entryPoint");
    }

    [TestMethod]
    [DataRow("quick-launcher")]
    [DataRow("quickLauncher.")]
    [DataRow(".quickLauncher")]
    [DataRow("quickLauncher.v2_beta")]
    public void InvalidPluginIdFails(string pluginId)
    {
        using var directory = TestPluginDirectory.Create();
        var manifestPath = directory.WriteManifest($$"""
            {
              "id": "{{pluginId}}",
              "displayName": "Quick Launcher Plus",
              "version": "0.1.0",
              "assembly": "QuickLauncherPlus.dll",
              "entryPoint": "Plugin"
            }
            """);

        var ex = ThrowsInvalidOperation(() => new MewooPluginManifestReader().Read(manifestPath));

        StringAssert.Contains(ex.Message, "invalid plugin id");
    }

    [TestMethod]
    public void RootedAssemblyPathFails()
    {
        using var directory = TestPluginDirectory.Create();
        var assemblyPath = Path.Combine(Path.GetPathRoot(directory.PluginDirectory)!, "Plugin.dll");
        var manifestPath = directory.WriteManifest($$"""
            {
              "id": "xkyss.quickLauncherPlus",
              "displayName": "Quick Launcher Plus",
              "version": "0.1.0",
              "assembly": "{{Escape(assemblyPath)}}",
              "entryPoint": "Plugin"
            }
            """);

        var ex = ThrowsInvalidOperation(() => new MewooPluginManifestReader().Read(manifestPath));

        StringAssert.Contains(ex.Message, "must be relative");
    }

    [TestMethod]
    public void EscapingAssemblyPathFails()
    {
        using var directory = TestPluginDirectory.Create();
        var manifestPath = directory.WriteManifest(
            """
            {
              "id": "xkyss.quickLauncherPlus",
              "displayName": "Quick Launcher Plus",
              "version": "0.1.0",
              "assembly": "../Plugin.dll",
              "entryPoint": "Plugin"
            }
            """);

        var ex = ThrowsInvalidOperation(() => new MewooPluginManifestReader().Read(manifestPath));

        StringAssert.Contains(ex.Message, "must stay inside");
    }

    private static string Escape(string value) => value.Replace("\\", "\\\\", StringComparison.Ordinal);

    private static InvalidOperationException ThrowsInvalidOperation(Action action)
    {
        try
        {
            action();
        }
        catch (InvalidOperationException ex)
        {
            return ex;
        }

        Assert.Fail("Expected InvalidOperationException.");
        throw new InvalidOperationException("Expected InvalidOperationException.");
    }

    private sealed class TestPluginDirectory : IDisposable
    {
        private TestPluginDirectory(string root)
        {
            Root = root;
            PluginDirectory = Path.Combine(root, "testPlugin");
            Directory.CreateDirectory(PluginDirectory);
        }

        public string Root { get; }

        public string PluginDirectory { get; }

        public static TestPluginDirectory Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "Mewoo.Core.Tests", Guid.NewGuid().ToString("N"));
            return new TestPluginDirectory(root);
        }

        public string WriteManifest(string json)
        {
            var manifestPath = Path.Combine(PluginDirectory, MewooRuntimePluginCatalog.ManifestFileName);
            File.WriteAllText(manifestPath, json);
            return manifestPath;
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
