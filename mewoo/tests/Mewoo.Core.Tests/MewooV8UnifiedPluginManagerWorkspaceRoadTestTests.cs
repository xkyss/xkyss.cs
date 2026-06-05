using Aprillz.MewUI.Controls;
using Mewoo.Abstractions;
using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Views;
using Mewoo.Core.Logging;
using Mewoo.Core.Plugins;
using Mewoo.Controls.Sidebar;
using Mewoo.Plugins.PluginManager;
using Mewoo.Plugins.Settings;
using Mewoo.Workbench;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class MewooV8UnifiedPluginManagerWorkspaceRoadTestTests
{
    private const string PluginManagerActivity = "pluginManager.activity";
    private const string SettingsActivity = "settings.activity";

    [TestMethod]
    public void UnifiedPluginManagerWorkspaceRoadTestCoversActivityBarSidebarAndMainViews()
    {
        var logger = new InMemoryMewooLogger();
        var host = new MewooPluginHost(logger);
        using var pluginRoot = new TestPluginRoot();
        pluginRoot.WriteInvalidRuntimeManifest("brokenRuntime");
        var runtimePlugins = new MewooRuntimePluginManager(logger);
        runtimePlugins.LoadDiscoveredPlugins(pluginRoot.Path, host);

        host.RegisterPlugin(
            new PluginManagerPlugin(
                runtimePlugins,
                host,
                EmptyServiceProvider.Instance,
                pluginRoot.Path,
                logger),
            MewooPluginRegistrationSource.BuiltIn);
        host.RegisterPlugin(new SettingsPlugin(), MewooPluginRegistrationSource.BuiltIn);

        var pluginManagerSnapshot = host.GetRegisteredContributions("pluginManager");
        var settingsSnapshot = host.GetRegisteredContributions("settings");
        var sections = WorkbenchActivityBarModel.CreateSections(
            [
                Activity("quickLauncher.activity", ActivityBarSection.Primary, 0),
                ..pluginManagerSnapshot.Activities,
                ..settingsSnapshot.Activities,
            ]);

        CollectionAssert.AreEqual(
            new[] { "quickLauncher.activity" },
            sections.Primary.Select(activity => activity.Id).ToArray());
        CollectionAssert.AreEqual(
            new[] { PluginManagerActivity, SettingsActivity },
            sections.System.Select(activity => activity.Id).ToArray());
        CollectionAssert.DoesNotContain(
            sections.Primary.Concat(sections.System).Select(activity => activity.Id).ToArray(),
            "runtimeDiagnostics.activity");

        CollectionAssert.AreEquivalent(
            new[]
            {
                "pluginManager.home",
                "pluginManager.details",
                "pluginManager.installPreview",
                "pluginManager.updatePreview",
                "pluginManager.runtimeStatus",
                "pluginManager.discoveryIssues",
                "pluginManager.operationLog",
            },
            pluginManagerSnapshot.MainViews.Select(view => view.Id).ToArray());

        var workbench = new TrackingWorkbenchService();
        var sidebarDescriptor = pluginManagerSnapshot.ViewContainers
            .Single(container => container.Id == "pluginManager.views")
            .Views
            .Single(view => view.Id == "pluginManager.installed");
        var sidebar = sidebarDescriptor.CreateView(new TestViewContext("pluginManager", workbench));
        var tree = FindTreeView(sidebar.NativeView);
        var manage = Nodes(tree).Single(node => NodeId(node) == "manage");
        var advanced = Nodes(tree).Single(node => NodeId(node) == "advanced");
        Assert.HasCount(1, runtimePlugins.DiscoveryIssues);

        CollectionAssert.AreEqual(
            new[] { "category.all", "category.enabled", "category.disabled", "category.needsAttention" },
            manage.Children.Select(NodeId).ToArray());
        Assert.IsFalse(RowState(advanced).IsExpanded);
        CollectionAssert.AreEqual(
            new[] { "advanced.runtimeStatus", "advanced.discoveryIssues", "advanced.operationLog" },
            advanced.Children.Select(NodeId).ToArray());

        tree.SelectedNode = manage.Children.Single(node => NodeId(node) == "category.needsAttention");
        CollectionAssert.AreEqual(new[] { "pluginManager.home" }, workbench.OpenedMainViewIds.ToArray());
        var home = pluginManagerSnapshot.MainViews
            .Single(view => view.Id == "pluginManager.home")
            .CreateView(new TestViewContext("pluginManager", workbench));
        CollectionAssert.Contains(Texts(home.NativeView).ToArray(), "Plugins: Needs Attention");

        var discoveryIssues = pluginManagerSnapshot.MainViews
            .Single(view => view.Id == "pluginManager.discoveryIssues")
            .CreateView(new TestViewContext("pluginManager", workbench));
        CollectionAssert.Contains(Texts(discoveryIssues.NativeView).ToArray(), "Discovery Issues");

        var state = new WorkbenchState();
        state.OpenMainView("pluginManager.home", PluginManagerActivity);
        state.OpenMainView("pluginManager.details", PluginManagerActivity);
        state.OpenMainView("pluginManager.installPreview", PluginManagerActivity);
        state.OpenMainView("pluginManager.updatePreview", PluginManagerActivity);
        state.OpenMainView("pluginManager.runtimeStatus", PluginManagerActivity);
        state.OpenMainView("pluginManager.discoveryIssues", PluginManagerActivity);
        state.OpenMainView("pluginManager.operationLog", PluginManagerActivity);
        state.OpenMainView("settings.home", SettingsActivity);

        state.SetActiveActivity(PluginManagerActivity);
        CollectionAssert.AreEqual(
            new[]
            {
                "pluginManager.home",
                "pluginManager.details",
                "pluginManager.installPreview",
                "pluginManager.updatePreview",
                "pluginManager.runtimeStatus",
                "pluginManager.discoveryIssues",
                "pluginManager.operationLog",
            },
            state.OpenMainViewIds.ToArray());
    }

    private static ActivityDescriptor Activity(string id, ActivityBarSection section, int order)
    {
        return new ActivityDescriptor(
            id,
            OwnerPluginId: id[..id.IndexOf('.', StringComparison.Ordinal)],
            Title: id,
            Icon: null,
            ViewContainerId: $"{id}.views",
            Order: order,
            Section: section);
    }

    private static IReadOnlyList<TreeViewNode> Nodes(TreeView treeView)
    {
        var nodes = (IReadOnlyList<TreeViewNode>?)treeView.Tag;
        Assert.IsNotNull(nodes);
        return nodes;
    }

    private static string NodeId(TreeViewNode node) => RowState(node).NodeId;

    private static SidebarNavigationRowState RowState(TreeViewNode node)
    {
        Assert.IsInstanceOfType<SidebarNavigationRowState>(node.Tag);
        return (SidebarNavigationRowState)node.Tag!;
    }

    private static TreeView FindTreeView(object nativeView)
    {
        if (nativeView is TreeView treeView)
        {
            return treeView;
        }

        foreach (var child in Children(nativeView))
        {
            try
            {
                return FindTreeView(child);
            }
            catch (InvalidOperationException)
            {
            }
        }

        throw new InvalidOperationException("TreeView not found.");
    }

    private static IEnumerable<string> Texts(object nativeView)
    {
        if (nativeView is TextBlock textBlock && !string.IsNullOrWhiteSpace(textBlock.Text))
        {
            yield return textBlock.Text;
        }

        foreach (var child in Children(nativeView))
        {
            foreach (var text in Texts(child))
            {
                yield return text;
            }
        }
    }

    private static IEnumerable<object> Children(object nativeView)
    {
        return nativeView switch
        {
            StackPanel stack => stack.Children,
            DockPanel dock => dock.Children,
            Border { Child: not null } border => [border.Child],
            _ => [],
        };
    }

    private sealed record TestViewContext(string PluginId, IWorkbenchService Workbench) : IMewooViewContext
    {
        public IServiceProvider Services => EmptyServiceProvider.Instance;
    }

    private sealed class TrackingWorkbenchService : IWorkbenchService
    {
        public List<string> OpenedMainViewIds { get; } = [];

        public WorkbenchViewState Current { get; private set; } = new(PluginManagerActivity, null);

        public event EventHandler<WorkbenchViewStateChangedEventArgs>? StateChanged;

        public ValueTask OpenMainViewAsync(string mainViewId, CancellationToken cancellationToken = default)
        {
            OpenedMainViewIds.Add(mainViewId);
            var previous = Current;
            Current = Current with { ActiveMainViewId = mainViewId };
            StateChanged?.Invoke(this, new WorkbenchViewStateChangedEventArgs(previous, Current));
            return ValueTask.CompletedTask;
        }

        public void UpdateStatusBarItem(string statusBarItemId, string text)
        {
        }

        public void OpenLogsPanel()
        {
        }
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

        public void WriteInvalidRuntimeManifest(string directoryName)
        {
            var directory = System.IO.Path.Combine(Path, directoryName);
            Directory.CreateDirectory(directory);
            File.WriteAllText(
                System.IO.Path.Combine(directory, MewooRuntimePluginCatalog.ManifestFileName),
                "{ invalid json");
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
