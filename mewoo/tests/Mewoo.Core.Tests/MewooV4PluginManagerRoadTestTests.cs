using System.IO.Compression;
using Mewoo.Abstractions;
using Mewoo.Abstractions.Plugins;
using Mewoo.Core.Plugins;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class MewooV4PluginManagerRoadTestTests
{
    private const string TestPluginAssemblyName = "Mewoo.TestPlugins.ValidRuntimePlugin.dll";
    private const string TestPluginId = "xkyss.validRuntimePlugin";
    private const string TestPluginEntryPoint = "Mewoo.TestPlugins.ValidRuntimePlugin.ValidRuntimePlugin";

    [TestMethod]
    public async Task V4LocalPackageRoadTestCoversPluginManagerFlows()
    {
        using var directory = TestV4RoadTestDirectory.Create();
        var packageOperations = new MewooPluginPackageOperations();
        var runtimePlugins = new MewooRuntimePluginManager();
        var pluginHost = new MewooPluginHost();
        var catalog = new MewooPluginManagerCatalog();

        var installResult = new MewooPluginInstaller().Install(
            directory.WriteRuntimePackage("0.1.0", "runtime-smoke-0.1.0"),
            directory.PluginRoot);
        Assert.IsTrue(installResult.Success, installResult.Issue?.ShortMessage);

        runtimePlugins.LoadDiscoveredPlugins(directory.PluginRoot, pluginHost);
        await runtimePlugins.ActivateLoadedPluginsAsync(pluginHost, plugin => new TestPluginContext(plugin.Id));

        var details = Entry(catalog, directory.PluginRoot, runtimePlugins, TestPluginId);
        Assert.AreEqual("0.1.0", details.Version);
        Assert.AreEqual("xkyss", details.Publisher);
        Assert.AreEqual("Trusted local code declared", details.TrustLabel);
        CollectionAssert.AreEqual(new[] { "Filesystem" }, details.PermissionLabels.ToArray());

        Assert.IsTrue(runtimePlugins.SetPluginDisabled(TestPluginId, disabled: true));
        await runtimePlugins.UnloadPluginAsync(TestPluginId, pluginHost);
        Assert.AreEqual(
            MewooPluginManagerCatalogState.Disabled,
            Entry(catalog, directory.PluginRoot, runtimePlugins, TestPluginId).State);

        Assert.IsTrue(runtimePlugins.SetPluginDisabled(TestPluginId, disabled: false));
        Assert.IsTrue(await runtimePlugins.ReloadPluginAsync(
            TestPluginId,
            directory.PluginRoot,
            pluginHost,
            plugin => new TestPluginContext(plugin.Id)));

        var updatePackage = directory.WriteRuntimePackage("0.2.0", "runtime-smoke-0.2.0");
        var updatePreview = new MewooPluginUpdatePreviewer().Preview(
            updatePackage,
            Entry(catalog, directory.PluginRoot, runtimePlugins, TestPluginId));
        Assert.IsTrue(updatePreview.Success, updatePreview.Message);
        Assert.AreEqual("Newer version", updatePreview.VersionComparisonLabel);

        var updateResult = await packageOperations.UpdateAsync(
            updatePackage,
            directory.PluginRoot,
            runtimePlugins,
            pluginHost,
            plugin => new TestPluginContext(plugin.Id));
        Assert.IsTrue(updateResult.Success, updateResult.Issue?.ShortMessage);
        Assert.AreEqual("0.2.0", Entry(catalog, directory.PluginRoot, runtimePlugins, TestPluginId).Version);

        var uninstallResult = await packageOperations.UninstallAsync(
            TestPluginId,
            directory.PluginRoot,
            runtimePlugins,
            pluginHost);
        Assert.IsTrue(uninstallResult.Success, uninstallResult.Issue?.ShortMessage);
        Assert.IsFalse(Directory.Exists(Path.Combine(directory.PluginRoot, TestPluginId)));

        var brokenDirectory = directory.CreateBrokenInstallDirectory("brokenInstall");
        var brokenEntry = catalog.CreateEntries(
                directory.PluginRoot,
                runtimePlugins.PluginStatuses,
                runtimePlugins.DiscoveryIssues)
            .Single(entry => string.Equals(entry.DisplayName, "brokenInstall", StringComparison.Ordinal));
        Assert.AreEqual(MewooPluginManagerCatalogState.Broken, brokenEntry.State);

        var removeBrokenResult = await packageOperations.RemoveBrokenInstallAsync(
            brokenEntry,
            directory.PluginRoot);
        Assert.IsTrue(removeBrokenResult.Success, removeBrokenResult.Issue?.ShortMessage);
        Assert.IsFalse(Directory.Exists(brokenDirectory));
    }

    private static MewooPluginManagerCatalogEntry Entry(
        MewooPluginManagerCatalog catalog,
        string pluginRoot,
        MewooRuntimePluginManager runtimePlugins,
        string pluginId) =>
        catalog.CreateEntries(pluginRoot, runtimePlugins.PluginStatuses, runtimePlugins.DiscoveryIssues)
            .Single(entry => string.Equals(entry.PluginId, pluginId, StringComparison.Ordinal));

    private sealed class TestV4RoadTestDirectory : IDisposable
    {
        private TestV4RoadTestDirectory(string root)
        {
            Root = root;
            PackageRoot = Path.Combine(root, "packages");
            PluginRoot = Path.Combine(root, "plugins");
            Directory.CreateDirectory(PackageRoot);
            Directory.CreateDirectory(PluginRoot);
        }

        public string Root { get; }

        public string PackageRoot { get; }

        public string PluginRoot { get; }

        public static TestV4RoadTestDirectory Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "Mewoo.Core.Tests", Guid.NewGuid().ToString("N"));
            return new TestV4RoadTestDirectory(root);
        }

        public string WriteRuntimePackage(string version, string packageFileName)
        {
            var packagePath = Path.Combine(PackageRoot, packageFileName + MewooPluginPackageFormat.Extension);
            using var archive = ZipFile.Open(packagePath, ZipArchiveMode.Create);
            WriteEntry(archive, MewooRuntimePluginCatalog.ManifestFileName, $$"""
                {
                  "id": "{{TestPluginId}}",
                  "displayName": "Valid Runtime Plugin",
                  "version": "{{version}}",
                  "assembly": "{{TestPluginAssemblyName}}",
                  "entryPoint": "{{TestPluginEntryPoint}}",
                  "trust": {
                    "trustedLocalCode": true,
                    "reason": "Road test sample plugin."
                  },
                  "permissions": [
                    {
                      "kind": "filesystem",
                      "reason": "Road test verifies permission surfacing."
                    }
                  ],
                  "metadata": {
                    "publisher": "xkyss",
                    "publisherDisplayName": "xkyss labs"
                  }
                }
                """);
            archive.CreateEntryFromFile(
                Path.Combine(AppContext.BaseDirectory, TestPluginAssemblyName),
                TestPluginAssemblyName);
            return packagePath;
        }

        public string CreateBrokenInstallDirectory(string name)
        {
            var directory = Path.Combine(PluginRoot, name);
            Directory.CreateDirectory(directory);
            return directory;
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
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
