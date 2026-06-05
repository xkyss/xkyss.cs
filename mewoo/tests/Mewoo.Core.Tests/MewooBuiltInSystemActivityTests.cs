using Aprillz.MewUI.Controls;
using Mewoo.Abstractions;
using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Views;
using Mewoo.Controls.Sidebar;
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

    [TestMethod]
    public void PluginManagerSidebarCategoriesDriveHomeViewCategory()
    {
        var host = new MewooPluginHost();
        using var pluginRoot = new TestPluginRoot();
        var workbench = new TrackingWorkbenchService();
        var plugin = new PluginManagerPlugin(
            new MewooRuntimePluginManager(),
            host,
            EmptyServiceProvider.Instance,
            pluginRoot.Path);
        host.RegisterPlugin(plugin, MewooPluginRegistrationSource.BuiltIn);
        var snapshot = host.GetRegisteredContributions("pluginManager");
        var sidebarDescriptor = snapshot.ViewContainers
            .Single(container => container.Id == "pluginManager.views")
            .Views
            .Single(view => view.Id == "pluginManager.installed");
        var homeDescriptor = snapshot.MainViews.Single(view => view.Id == "pluginManager.home");

        var sidebar = sidebarDescriptor.CreateView(new TestViewContext("pluginManager", workbench));
        var tree = FindTreeView(sidebar.NativeView);
        var manage = Nodes(tree).Single(node => NodeId(node) == "manage");
        var advanced = Nodes(tree).Single(node => NodeId(node) == "advanced");

        CollectionAssert.AreEqual(
            new[] { "category.all", "category.enabled", "category.disabled", "category.needsAttention" },
            manage.Children.Select(NodeId).ToArray());
        CollectionAssert.AreEqual(
            new[] { "advanced.runtimeStatus", "advanced.discoveryIssues", "advanced.operationLog" },
            advanced.Children.Select(NodeId).ToArray());
        Assert.IsFalse(RowState(advanced).IsExpanded);

        tree.SelectedNode = manage.Children.Single(node => NodeId(node) == "category.disabled");

        CollectionAssert.AreEqual(new[] { "pluginManager.home" }, workbench.OpenedMainViewIds.ToArray());
        var home = homeDescriptor.CreateView(new TestViewContext("pluginManager", workbench));
        CollectionAssert.Contains(Texts(home.NativeView).ToArray(), "Plugins: Disabled");
    }

    [TestMethod]
    public void PluginManagerContributesAdvancedDiagnosticViews()
    {
        var host = new MewooPluginHost();
        using var pluginRoot = new TestPluginRoot();
        var plugin = new PluginManagerPlugin(
            new MewooRuntimePluginManager(),
            host,
            EmptyServiceProvider.Instance,
            pluginRoot.Path);
        host.RegisterPlugin(plugin, MewooPluginRegistrationSource.BuiltIn);
        var snapshot = host.GetRegisteredContributions("pluginManager");

        CollectionAssert.Contains(snapshot.MainViews.Select(view => view.Id).ToArray(), "pluginManager.runtimeStatus");
        CollectionAssert.Contains(snapshot.MainViews.Select(view => view.Id).ToArray(), "pluginManager.discoveryIssues");
        CollectionAssert.Contains(snapshot.MainViews.Select(view => view.Id).ToArray(), "pluginManager.operationLog");

        var runtimeStatus = snapshot.MainViews
            .Single(view => view.Id == "pluginManager.runtimeStatus")
            .CreateView(new TestViewContext("pluginManager", new TrackingWorkbenchService()));

        CollectionAssert.Contains(Texts(runtimeStatus.NativeView).ToArray(), "Runtime Status");
    }

    private static IReadOnlyList<TreeViewNode> Nodes(TreeView treeView)
    {
        var nodes = (IReadOnlyList<TreeViewNode>?)treeView.Tag;
        Assert.IsNotNull(nodes);
        return nodes;
    }

    private static string NodeId(TreeViewNode node)
    {
        return RowState(node).NodeId;
    }

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

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public static readonly EmptyServiceProvider Instance = new();

        public object? GetService(Type serviceType) => null;
    }

    private sealed record TestViewContext(string PluginId, IWorkbenchService Workbench) : IMewooViewContext
    {
        public IServiceProvider Services => EmptyServiceProvider.Instance;
    }

    private sealed class TrackingWorkbenchService : IWorkbenchService
    {
        public List<string> OpenedMainViewIds { get; } = [];

        public WorkbenchViewState Current { get; private set; } = new("pluginManager.activity", null);

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
