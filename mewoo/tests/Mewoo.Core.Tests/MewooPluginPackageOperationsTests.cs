using System.IO.Compression;
using Mewoo.Abstractions;
using Mewoo.Abstractions.Plugins;
using Mewoo.Core.Plugins;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class MewooPluginPackageOperationsTests
{
    private const string TestPluginAssemblyName = "Mewoo.TestPlugins.ValidRuntimePlugin.dll";
    private const string TestPluginId = "xkyss.validRuntimePlugin";
    private const string TestPluginEntryPoint = "Mewoo.TestPlugins.ValidRuntimePlugin.ValidRuntimePlugin";

    [TestMethod]
    public async Task UninstallUnloadsPluginBeforeDeletingInstalledDirectory()
    {
        using var directory = TestPackageOperationsDirectory.Create();
        new MewooPluginInstaller().Install(directory.WriteRuntimePackage("0.1.0"), directory.PluginRoot);
        var logger = new Mewoo.Core.Logging.InMemoryMewooLogger();
        var runtimePlugins = new MewooRuntimePluginManager(logger);
        var pluginHost = new MewooPluginHost(logger);
        runtimePlugins.LoadDiscoveredPlugins(directory.PluginRoot, pluginHost);
        await runtimePlugins.ActivateLoadedPluginsAsync(pluginHost, plugin => new TestPluginContext(plugin.Id));

        var result = await new MewooPluginPackageOperations().UninstallAsync(
            TestPluginId,
            directory.PluginRoot,
            runtimePlugins,
            pluginHost);

        Assert.IsTrue(result.Success, result.Issue?.ShortMessage);
        Assert.IsFalse(Directory.Exists(Path.Combine(directory.PluginRoot, TestPluginId)));
        Assert.AreEqual(0, runtimePlugins.LoadedPlugins.Count);
        Assert.AreEqual(0, pluginHost.Plugins.Count);
        Assert.IsFalse(pluginHost.VisibleContributions.Activities.Any(item =>
            item.Id == "xkyss.validRuntimePlugin.activity"));
    }

    [TestMethod]
    public async Task UpdateInstallsNewPackageVersion()
    {
        using var directory = TestPackageOperationsDirectory.Create();
        new MewooPluginInstaller().Install(directory.WriteRuntimePackage("0.1.0"), directory.PluginRoot);
        var runtimePlugins = new MewooRuntimePluginManager();
        var pluginHost = new MewooPluginHost();

        var result = await new MewooPluginPackageOperations().UpdateAsync(
            directory.WriteRuntimePackage("0.2.0", packageFileName: "update"),
            directory.PluginRoot,
            runtimePlugins,
            pluginHost,
            plugin => new TestPluginContext(plugin.Id));

        Assert.IsTrue(result.Success);
        Assert.AreEqual("0.2.0", runtimePlugins.PluginStatuses[0].Descriptor.Manifest.Version);
        Assert.IsTrue(pluginHost.VisibleContributions.Activities.Any(item =>
            item.Id == "xkyss.validRuntimePlugin.activity"));
    }

    [TestMethod]
    public async Task UpdateFailurePreservesPreviousPackage()
    {
        using var directory = TestPackageOperationsDirectory.Create();
        new MewooPluginInstaller().Install(directory.WriteRuntimePackage("0.1.0"), directory.PluginRoot);

        var result = await new MewooPluginPackageOperations().UpdateAsync(
            directory.WriteRuntimePackage("0.2.0", packageFileName: "bad-update", extraEntries: [("../escape.txt", "bad")]),
            directory.PluginRoot,
            new MewooRuntimePluginManager(),
            new MewooPluginHost(),
            plugin => new TestPluginContext(plugin.Id));

        Assert.IsFalse(result.Success);
        var manifestPath = Path.Combine(directory.PluginRoot, TestPluginId, MewooRuntimePluginCatalog.ManifestFileName);
        StringAssert.Contains(File.ReadAllText(manifestPath), "\"version\": \"0.1.0\"");
        Assert.IsFalse(File.Exists(Path.Combine(directory.Root, "escape.txt")));
    }

    [TestMethod]
    public async Task UpdateLoadedPluginReplacesInstalledDirectoryAndReloadsContributions()
    {
        using var directory = TestPackageOperationsDirectory.Create();
        new MewooPluginInstaller().Install(
            directory.WriteRuntimePackage("0.1.0", extraEntries: [("OldDependency.dll", "old")]),
            directory.PluginRoot);
        var runtimePlugins = new MewooRuntimePluginManager();
        var pluginHost = new MewooPluginHost();
        runtimePlugins.LoadDiscoveredPlugins(directory.PluginRoot, pluginHost);
        await runtimePlugins.ActivateLoadedPluginsAsync(pluginHost, plugin => new TestPluginContext(plugin.Id));

        var result = await new MewooPluginPackageOperations().UpdateAsync(
            directory.WriteRuntimePackage("0.2.0", packageFileName: "loaded-update", extraEntries: [("NewDependency.dll", "new")]),
            directory.PluginRoot,
            runtimePlugins,
            pluginHost,
            plugin => new TestPluginContext(plugin.Id));

        Assert.IsTrue(result.Success);
        Assert.IsTrue(pluginHost.VisibleContributions.StatusBarItems.Any(item =>
            item.Id == "xkyss.validRuntimePlugin.status"));
        Assert.AreEqual("0.2.0", runtimePlugins.PluginStatuses[0].Descriptor.Manifest.Version);
        Assert.IsTrue(File.Exists(Path.Combine(directory.PluginRoot, TestPluginId, "NewDependency.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(directory.PluginRoot, TestPluginId, "OldDependency.dll")));
    }

    private sealed class TestPackageOperationsDirectory : IDisposable
    {
        private TestPackageOperationsDirectory(string root)
        {
            Root = root;
            PackageRoot = Path.Combine(root, "packages");
            PluginRoot = Path.Combine(root, "plugins");
            Directory.CreateDirectory(PackageRoot);
        }

        public string Root { get; }

        public string PackageRoot { get; }

        public string PluginRoot { get; }

        public static TestPackageOperationsDirectory Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "Mewoo.Core.Tests", Guid.NewGuid().ToString("N"));
            return new TestPackageOperationsDirectory(root);
        }

        public string WriteRuntimePackage(
            string version,
            string packageFileName = "runtime",
            params (string EntryName, string Contents)[] extraEntries)
        {
            var packagePath = Path.Combine(PackageRoot, packageFileName + MewooPluginPackageFormat.Extension);
            using var archive = ZipFile.Open(packagePath, ZipArchiveMode.Create);
            WriteEntry(archive, MewooRuntimePluginCatalog.ManifestFileName, $$"""
                {
                  "id": "{{TestPluginId}}",
                  "displayName": "Valid Runtime Plugin",
                  "version": "{{version}}",
                  "assembly": "{{TestPluginAssemblyName}}",
                  "entryPoint": "{{TestPluginEntryPoint}}"
                }
                """);
            archive.CreateEntryFromFile(
                Path.Combine(AppContext.BaseDirectory, TestPluginAssemblyName),
                TestPluginAssemblyName);

            foreach (var entry in extraEntries)
            {
                WriteEntry(archive, entry.EntryName, entry.Contents);
            }

            return packagePath;
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                TryDeleteRoot();
            }
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

        private static void WriteEntry(ZipArchive archive, string entryName, string contents)
        {
            var entry = archive.CreateEntry(entryName);
            using var writer = new StreamWriter(entry.Open());
            writer.Write(contents);
        }
    }

    private sealed record TestPluginContext(
        string PluginId,
        IServiceProvider? Services = null,
        IWorkbenchService? Workbench = null) : IMewooPluginContext
    {
        IServiceProvider IMewooPluginContext.Services => Services ?? EmptyServiceProvider.Instance;

        IWorkbenchService IMewooPluginContext.Workbench => Workbench ?? EmptyWorkbenchService.Instance;
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public static readonly EmptyServiceProvider Instance = new();

        public object? GetService(Type serviceType) => null;
    }

    private sealed class EmptyWorkbenchService : IWorkbenchService
    {
        public static readonly EmptyWorkbenchService Instance = new();

        public ValueTask OpenMainViewAsync(string mainViewId, CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;

        public void UpdateStatusBarItem(string statusBarItemId, string text)
        {
        }

        public void OpenLogsPanel()
        {
        }
    }
}
