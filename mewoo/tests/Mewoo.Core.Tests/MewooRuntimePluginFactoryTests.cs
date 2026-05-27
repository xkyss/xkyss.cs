using Mewoo.Abstractions.Plugins;
using Mewoo.Core.Logging;
using Mewoo.Core.Plugins;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class MewooRuntimePluginFactoryTests
{
    private const string TestPluginAssemblyName = "Mewoo.TestPlugins.ValidRuntimePlugin.dll";
    private const string ValidEntryPoint = "Mewoo.TestPlugins.ValidRuntimePlugin.ValidRuntimePlugin";

    [TestMethod]
    public void TryCreateInstantiatesEntryPointPlugin()
    {
        using var directory = RuntimePluginTestDirectory.Create();
        var descriptor = directory.CreateDescriptor(ValidEntryPoint);

        var loaded = new MewooRuntimePluginFactory().TryCreate(descriptor);

        Assert.IsNotNull(loaded);
        Assert.AreEqual("xkyss.validRuntimePlugin", loaded.Plugin.Id);
        loaded.LoadContext.Unload();
    }

    [TestMethod]
    public void TryCreateRejectsMissingEntryPoint()
    {
        using var directory = RuntimePluginTestDirectory.Create();
        var descriptor = directory.CreateDescriptor("Mewoo.TestPlugins.ValidRuntimePlugin.MissingPlugin");
        var logger = new InMemoryMewooLogger();

        var loaded = new MewooRuntimePluginFactory(logger).TryCreate(descriptor);

        Assert.IsNull(loaded);
        Assert.IsTrue(logger.Entries.Any(entry =>
            entry.Message.Contains("Failed to create runtime plugin", StringComparison.Ordinal)
            && entry.Exception?.Contains("was not found", StringComparison.Ordinal) == true));
    }

    [TestMethod]
    public void TryCreateRejectsEntryPointThatIsNotPlugin()
    {
        using var directory = RuntimePluginTestDirectory.Create();
        var descriptor = directory.CreateDescriptor("Mewoo.TestPlugins.ValidRuntimePlugin.NotAPlugin");
        var logger = new InMemoryMewooLogger();

        var loaded = new MewooRuntimePluginFactory(logger).TryCreate(descriptor);

        Assert.IsNull(loaded);
        Assert.IsTrue(logger.Entries.Any(entry =>
            entry.Exception?.Contains("does not implement IMewooPlugin", StringComparison.Ordinal) == true));
    }

    [TestMethod]
    public void TryCreateRejectsManifestIdMismatch()
    {
        using var directory = RuntimePluginTestDirectory.Create();
        var descriptor = directory.CreateDescriptor("Mewoo.TestPlugins.ValidRuntimePlugin.DifferentIdRuntimePlugin");
        var logger = new InMemoryMewooLogger();

        var loaded = new MewooRuntimePluginFactory(logger).TryCreate(descriptor);

        Assert.IsNull(loaded);
        Assert.IsTrue(logger.Entries.Any(entry =>
            entry.Exception?.Contains("does not match plugin id", StringComparison.Ordinal) == true));
    }

    [TestMethod]
    public void TryCreateLogsConstructorFailure()
    {
        using var directory = RuntimePluginTestDirectory.Create();
        var descriptor = directory.CreateDescriptor("Mewoo.TestPlugins.ValidRuntimePlugin.ThrowingRuntimePlugin");
        var logger = new InMemoryMewooLogger();

        var loaded = new MewooRuntimePluginFactory(logger).TryCreate(descriptor);

        Assert.IsNull(loaded);
        Assert.IsTrue(logger.Entries.Any(entry =>
            entry.Exception?.Contains("Constructor failed", StringComparison.Ordinal) == true));
    }

    private sealed class RuntimePluginTestDirectory : IDisposable
    {
        private RuntimePluginTestDirectory(string root)
        {
            Root = root;
            PluginDirectory = Path.Combine(root, "runtimePlugin");
            Directory.CreateDirectory(PluginDirectory);
            CopyTestPluginAssembly();
        }

        public string Root { get; }

        public string PluginDirectory { get; }

        public static RuntimePluginTestDirectory Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "Mewoo.Core.Tests", Guid.NewGuid().ToString("N"));
            return new RuntimePluginTestDirectory(root);
        }

        public MewooRuntimePluginDescriptor CreateDescriptor(string entryPoint)
        {
            var manifest = new MewooPluginManifest
            {
                Id = "xkyss.validRuntimePlugin",
                DisplayName = "Valid Runtime Plugin",
                Version = "0.1.0",
                Assembly = TestPluginAssemblyName,
                EntryPoint = entryPoint,
            };

            return new MewooRuntimePluginDescriptor(
                manifest,
                Path.Combine(PluginDirectory, MewooRuntimePluginCatalog.ManifestFileName),
                PluginDirectory,
                Path.Combine(PluginDirectory, TestPluginAssemblyName));
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                TryDeleteRoot();
            }
        }

        private void CopyTestPluginAssembly()
        {
            var source = Path.Combine(AppContext.BaseDirectory, TestPluginAssemblyName);
            var target = Path.Combine(PluginDirectory, TestPluginAssemblyName);
            File.Copy(source, target);
        }

        private void TryDeleteRoot()
        {
            for (var attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    Directory.Delete(Root, recursive: true);
                    return;
                }
                catch (IOException)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                catch (UnauthorizedAccessException)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }
    }
}

