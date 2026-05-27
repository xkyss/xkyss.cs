using Mewoo.Abstractions;
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
        var pluginHost = new MewooPluginHost();
        var manager = new MewooRuntimePluginManager();
        manager.LoadDiscoveredPlugins(directory.Root, pluginHost);
        await pluginHost.ActivateAllAsync(plugin => new TestPluginContext(plugin.Id));

        var unloaded = await manager.UnloadPluginAsync("xkyss.validRuntimePlugin", pluginHost);

        Assert.IsTrue(unloaded);
        Assert.AreEqual(0, manager.LoadedPlugins.Count);
        Assert.IsFalse(pluginHost.Commands.Contains("xkyss.validRuntimePlugin.ping"));
        Assert.IsFalse(pluginHost.VisibleContributions.StatusBarItems.Any(item =>
            item.Id == "xkyss.validRuntimePlugin.status"));
        Assert.AreEqual(MewooPluginState.Unloaded, pluginHost.Plugins.Single().State);
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

        public void WriteValidRuntimePlugin()
        {
            var source = Path.Combine(AppContext.BaseDirectory, TestPluginAssemblyName);
            File.Copy(source, Path.Combine(PluginDirectory, TestPluginAssemblyName));
            File.WriteAllText(
                Path.Combine(PluginDirectory, MewooRuntimePluginCatalog.ManifestFileName),
                $$"""
                {
                  "id": "xkyss.validRuntimePlugin",
                  "displayName": "Valid Runtime Plugin",
                  "version": "0.1.0",
                  "assembly": "{{TestPluginAssemblyName}}",
                  "entryPoint": "Mewoo.TestPlugins.ValidRuntimePlugin.ValidRuntimePlugin"
                }
                """);
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
    }
}
