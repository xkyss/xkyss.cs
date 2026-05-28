using Mewoo.Abstractions;
using Mewoo.Abstractions.Commands;
using Mewoo.Abstractions.Plugins;
using Mewoo.Core.Logging;
using Mewoo.Core.Plugins;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class MewooRuntimePluginManagerTests
{
    private const string TestPluginAssemblyName = "Mewoo.TestPlugins.ValidRuntimePlugin.dll";

    [TestMethod]
    public async Task LoadDiscoveredPluginsRegistersAndActivatesRuntimePlugin()
    {
        using var directory = RuntimePluginRoot.Create();
        directory.WriteValidRuntimePlugin();
        var logger = new InMemoryMewooLogger();
        var pluginHost = new MewooPluginHost(logger);
        var manager = new MewooRuntimePluginManager(logger);

        var loaded = manager.LoadDiscoveredPlugins(directory.Root, pluginHost);
        await pluginHost.ActivateAllAsync(plugin => new TestPluginContext(plugin.Id));

        Assert.AreEqual(1, loaded.Count);
        Assert.AreEqual(1, manager.LoadedPlugins.Count);
        Assert.IsTrue(pluginHost.Commands.Contains("xkyss.validRuntimePlugin.ping"));
        Assert.IsTrue(pluginHost.VisibleContributions.StatusBarItems.Any(item =>
            item.Id == "xkyss.validRuntimePlugin.status"));
    }

    [TestMethod]
    public async Task UnloadPluginRemovesRuntimeContributionsAndReleasesManagerReference()
    {
        using var directory = RuntimePluginRoot.Create();
        directory.WriteValidRuntimePlugin();
        var logger = new InMemoryMewooLogger();
        var pluginHost = new MewooPluginHost(logger);
        var manager = new MewooRuntimePluginManager(logger);
        manager.LoadDiscoveredPlugins(directory.Root, pluginHost);
        await pluginHost.ActivateAllAsync(plugin => new TestPluginContext(plugin.Id));

        var unloaded = await manager.UnloadPluginAsync("xkyss.validRuntimePlugin", pluginHost);

        Assert.IsTrue(unloaded);
        Assert.AreEqual(0, manager.LoadedPlugins.Count);
        Assert.IsFalse(pluginHost.Commands.Contains("xkyss.validRuntimePlugin.ping"));
        Assert.IsFalse(pluginHost.VisibleContributions.StatusBarItems.Any(item =>
            item.Id == "xkyss.validRuntimePlugin.status"));
        Assert.AreEqual(0, pluginHost.Plugins.Count);
        Assert.IsTrue(logger.Entries.Any(entry =>
            entry.Message.Contains("load context was collected", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void LoadDiscoveredPluginsDoesNotLoadDisabledPlugins()
    {
        using var directory = RuntimePluginRoot.Create();
        directory.WriteValidRuntimePlugin(disabled: true);
        var pluginHost = new MewooPluginHost();
        var manager = new MewooRuntimePluginManager();

        var loaded = manager.LoadDiscoveredPlugins(directory.Root, pluginHost);

        Assert.AreEqual(0, loaded.Count);
        Assert.AreEqual(1, manager.PluginStatuses.Count);
        Assert.AreEqual(MewooRuntimePluginState.Disabled, manager.PluginStatuses[0].State);
        Assert.AreEqual(MewooRuntimePluginIssueCategory.Disabled, manager.PluginStatuses[0].IssueCategory);
        Assert.AreEqual("Disabled", manager.PluginStatuses[0].CategoryLabel);
        Assert.AreEqual(0, pluginHost.Plugins.Count);
    }

    [TestMethod]
    public void LoadDiscoveredPluginsDoesNotLoadIncompatiblePlugins()
    {
        using var directory = RuntimePluginRoot.Create();
        directory.WriteValidRuntimePlugin(minimumMewooVersion: "99.0.0");
        var pluginHost = new MewooPluginHost();
        var manager = new MewooRuntimePluginManager(currentVersion: new Version(1, 0, 0));

        var loaded = manager.LoadDiscoveredPlugins(directory.Root, pluginHost);

        Assert.AreEqual(0, loaded.Count);
        Assert.AreEqual(1, manager.PluginStatuses.Count);
        Assert.AreEqual(MewooRuntimePluginState.Incompatible, manager.PluginStatuses[0].State);
        Assert.AreEqual(MewooRuntimePluginIssueCategory.Compatibility, manager.PluginStatuses[0].IssueCategory);
        Assert.AreEqual("Compatibility", manager.PluginStatuses[0].CategoryLabel);
        Assert.AreEqual(0, pluginHost.Plugins.Count);
    }

    [TestMethod]
    public void LoadDiscoveredPluginsRecordsMissingAssemblyFailureDetails()
    {
        using var directory = RuntimePluginRoot.Create();
        directory.WriteValidRuntimePlugin(copyAssembly: false);
        var pluginHost = new MewooPluginHost();
        var manager = new MewooRuntimePluginManager();

        var loaded = manager.LoadDiscoveredPlugins(directory.Root, pluginHost);

        Assert.AreEqual(0, loaded.Count);
        Assert.AreEqual(1, manager.PluginStatuses.Count);
        Assert.AreEqual(MewooRuntimePluginState.Failed, manager.PluginStatuses[0].State);
        Assert.AreEqual(MewooRuntimePluginIssueCategory.Assembly, manager.PluginStatuses[0].IssueCategory);
        Assert.AreEqual("Assembly", manager.PluginStatuses[0].CategoryLabel);
        StringAssert.Contains(manager.PluginStatuses[0].ShortMessage, TestPluginAssemblyName);
        Assert.AreEqual(0, pluginHost.Plugins.Count);
    }

    [TestMethod]
    public async Task ActivateLoadedPluginsRecordsActivationFailureDetails()
    {
        using var directory = RuntimePluginRoot.Create();
        directory.WriteActivatingRuntimePlugin();
        var logger = new InMemoryMewooLogger();
        var pluginHost = new MewooPluginHost(logger);
        var manager = new MewooRuntimePluginManager(logger);
        manager.LoadDiscoveredPlugins(directory.Root, pluginHost);

        await manager.ActivateLoadedPluginsAsync(pluginHost, plugin => new TestPluginContext(plugin.Id));

        Assert.AreEqual(1, manager.PluginStatuses.Count);
        Assert.AreEqual(MewooRuntimePluginState.Failed, manager.PluginStatuses[0].State);
        Assert.AreEqual(MewooRuntimePluginIssueCategory.Activation, manager.PluginStatuses[0].IssueCategory);
        Assert.AreEqual("Activation", manager.PluginStatuses[0].CategoryLabel);
        StringAssert.Contains(manager.PluginStatuses[0].ShortMessage, "Activation failed.");
    }

    [TestMethod]
    public async Task RuntimePluginRoadTestCoversContributionsOperationsAndFailures()
    {
        using var directory = RuntimePluginRoot.Create();
        directory.WriteValidRuntimePlugin();
        var logger = new InMemoryMewooLogger();
        var pluginHost = new MewooPluginHost(logger);
        var manager = new MewooRuntimePluginManager(logger);
        var workbench = new TrackingWorkbenchService();

        var loaded = manager.LoadDiscoveredPlugins(directory.Root, pluginHost);
        await manager.ActivateLoadedPluginsAsync(
            pluginHost,
            plugin => new TestPluginContext(plugin.Id, Workbench: workbench));

        Assert.AreEqual(1, loaded.Count);
        Assert.IsTrue(pluginHost.VisibleContributions.Activities.Any(item =>
            item.Id == "xkyss.validRuntimePlugin.activity"));
        Assert.IsTrue(pluginHost.VisibleContributions.ViewContainers.Any(item =>
            item.Id == "xkyss.validRuntimePlugin.views"));
        Assert.IsTrue(pluginHost.VisibleContributions.MainViews.Any(item =>
            item.Id == "xkyss.validRuntimePlugin.main"));
        Assert.IsTrue(pluginHost.Commands.Contains("xkyss.validRuntimePlugin.ping"));
        Assert.IsTrue(pluginHost.VisibleContributions.StatusBarItems.Any(item =>
            item.Id == "xkyss.validRuntimePlugin.status"));
        Assert.AreEqual(MewooRuntimePluginState.Registered, manager.PluginStatuses[0].State);

        await pluginHost.Commands.ExecuteAsync(
            "xkyss.validRuntimePlugin.ping",
            new TestCommandContext(EmptyServiceProvider.Instance, workbench));

        Assert.IsTrue(manager.SetPluginDisabled("xkyss.validRuntimePlugin", disabled: true));
        Assert.IsTrue(await manager.UnloadPluginAsync("xkyss.validRuntimePlugin", pluginHost));
        Assert.AreEqual(MewooRuntimePluginState.Disabled, manager.PluginStatuses[0].State);
        Assert.IsFalse(pluginHost.VisibleContributions.Activities.Any(item =>
            item.Id == "xkyss.validRuntimePlugin.activity"));

        Assert.IsTrue(manager.SetPluginDisabled("xkyss.validRuntimePlugin", disabled: false));
        Assert.IsTrue(await manager.ReloadPluginAsync(
            "xkyss.validRuntimePlugin",
            directory.Root,
            pluginHost,
            plugin => new TestPluginContext(plugin.Id, Workbench: workbench)));
        Assert.IsTrue(pluginHost.VisibleContributions.Activities.Any(item =>
            item.Id == "xkyss.validRuntimePlugin.activity"));

        Assert.IsTrue(await manager.UnloadPluginAsync("xkyss.validRuntimePlugin", pluginHost));
        Assert.IsTrue(await manager.ReloadPluginAsync(
            "xkyss.validRuntimePlugin",
            directory.Root,
            pluginHost,
            plugin => new TestPluginContext(plugin.Id, Workbench: workbench)));

        using var incompatibleDirectory = RuntimePluginRoot.Create();
        incompatibleDirectory.WriteValidRuntimePlugin(minimumMewooVersion: "99.0.0");
        var incompatibleManager = new MewooRuntimePluginManager(currentVersion: new Version(1, 0, 0));
        var incompatibleLoaded = incompatibleManager.LoadDiscoveredPlugins(incompatibleDirectory.Root, new MewooPluginHost());
        Assert.AreEqual(0, incompatibleLoaded.Count);
        Assert.AreEqual(MewooRuntimePluginState.Incompatible, incompatibleManager.PluginStatuses[0].State);

        using var invalidDirectory = RuntimePluginRoot.Create();
        invalidDirectory.WriteInvalidManifest("invalidPlugin");
        invalidDirectory.WriteValidRuntimePlugin();
        var invalidManager = new MewooRuntimePluginManager();
        var invalidLoaded = invalidManager.LoadDiscoveredPlugins(invalidDirectory.Root, new MewooPluginHost());
        Assert.AreEqual(1, invalidLoaded.Count);
        Assert.AreEqual(1, invalidManager.DiscoveryIssues.Count);
        Assert.AreEqual(MewooRuntimePluginIssueCategory.Manifest, invalidManager.DiscoveryIssues[0].Category);
    }

    private sealed class RuntimePluginRoot : IDisposable
    {
        private RuntimePluginRoot(string root)
        {
            Root = root;
            PluginDirectory = Path.Combine(root, "validRuntimePlugin");
            Directory.CreateDirectory(PluginDirectory);
        }

        public string Root { get; }

        public string PluginDirectory { get; }

        public static RuntimePluginRoot Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "Mewoo.Core.Tests", Guid.NewGuid().ToString("N"));
            return new RuntimePluginRoot(root);
        }

        public void WriteValidRuntimePlugin(
            bool disabled = false,
            string? minimumMewooVersion = null,
            bool copyAssembly = true)
        {
            WriteRuntimePlugin(
                "xkyss.validRuntimePlugin",
                "Valid Runtime Plugin",
                "Mewoo.TestPlugins.ValidRuntimePlugin.ValidRuntimePlugin",
                disabled,
                minimumMewooVersion,
                copyAssembly);
        }

        public void WriteActivatingRuntimePlugin()
        {
            WriteRuntimePlugin(
                "xkyss.activatingRuntimePlugin",
                "Activating Runtime Plugin",
                "Mewoo.TestPlugins.ValidRuntimePlugin.ActivatingRuntimePlugin");
        }

        public void WriteInvalidManifest(string directoryName)
        {
            var pluginDirectory = Path.Combine(Root, directoryName);
            Directory.CreateDirectory(pluginDirectory);
            File.WriteAllText(
                Path.Combine(pluginDirectory, MewooRuntimePluginCatalog.ManifestFileName),
                """
                {
                  "id": "bad-id",
                  "displayName": "Invalid Plugin",
                  "version": "0.1.0",
                  "assembly": "InvalidPlugin.dll",
                  "entryPoint": "Plugin"
                }
                """);
        }

        private void WriteRuntimePlugin(
            string pluginId,
            string displayName,
            string entryPoint,
            bool disabled = false,
            string? minimumMewooVersion = null,
            bool copyAssembly = true)
        {
            var source = Path.Combine(AppContext.BaseDirectory, TestPluginAssemblyName);
            if (copyAssembly)
            {
                File.Copy(source, Path.Combine(PluginDirectory, TestPluginAssemblyName));
            }

            File.WriteAllText(
                Path.Combine(PluginDirectory, MewooRuntimePluginCatalog.ManifestFileName),
                $$"""
                {
                  "id": "{{pluginId}}",
                  "displayName": "{{displayName}}",
                  "version": "0.1.0",
                  "assembly": "{{TestPluginAssemblyName}}",
                  "entryPoint": "{{entryPoint}}",
                  "minimumMewooVersion": {{JsonStringOrNull(minimumMewooVersion)}},
                  "disabled": {{disabled.ToString().ToLowerInvariant()}}
                }
                """);
        }

        private static string JsonStringOrNull(string? value)
        {
            return value is null ? "null" : $"\"{value}\"";
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

        public ValueTask OpenMainViewAsync(string mainViewId, CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public void UpdateStatusBarItem(string statusBarItemId, string text)
        {
        }

        public void OpenLogsPanel()
        {
        }
    }

    private sealed record TestCommandContext(IServiceProvider Services, IWorkbenchService Workbench)
        : IMewooCommandContext;

    private sealed class TrackingWorkbenchService : IWorkbenchService
    {
        public List<string> OpenedMainViews { get; } = [];

        public Dictionary<string, string> StatusUpdates { get; } = new(StringComparer.Ordinal);

        public bool LogsOpened { get; private set; }

        public ValueTask OpenMainViewAsync(string mainViewId, CancellationToken cancellationToken = default)
        {
            OpenedMainViews.Add(mainViewId);
            return ValueTask.CompletedTask;
        }

        public void UpdateStatusBarItem(string statusBarItemId, string text)
        {
            StatusUpdates[statusBarItemId] = text;
        }

        public void OpenLogsPanel()
        {
            LogsOpened = true;
        }
    }
}
