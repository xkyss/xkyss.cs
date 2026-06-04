using Aprillz.MewUI.Controls;
using Mewoo.Abstractions;
using Mewoo.Controls.Sidebar;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class SidebarNavigationControlsTests
{
    [TestMethod]
    public void UncontrolledNavigationAutoExpandsAncestorsOfSelectedMainView()
    {
        var workbench = new TestWorkbenchService(
            new WorkbenchViewState("pluginManager.activity", "pluginManager.details"));

        var host = SidebarNavigation.Create(workbench)
            .Node("plugins", "Plugins", plugins => plugins
                .Node("installed", "Installed", installed => installed
                    .MainView("details", "Details", "pluginManager.details")))
            .Build();

        var plugins = Nodes(host)[0];
        var installed = plugins.Children[0];
        var details = installed.Children[0];

        Assert.IsTrue(host.IsExpanded(plugins));
        Assert.IsTrue(host.IsExpanded(installed));
        Assert.AreSame(details, host.SelectedNode);
        AssertRow(plugins, "plugins", depth: 0, isExpanded: true, isSelected: false, containsActive: true);
        AssertRow(installed, "installed", depth: 1, isExpanded: true, isSelected: false, containsActive: true);
        AssertRow(details, "details", depth: 2, isExpanded: false, isSelected: true, containsActive: false);
    }

    [TestMethod]
    public void ControlledNavigationReportsExpansionWithoutMutatingRenderedState()
    {
        var workbench = new TestWorkbenchService(new WorkbenchViewState(null, null));
        IReadOnlySet<string>? reportedExpansion = null;
        var host = SidebarNavigation.Create(
                workbench,
                new SidebarNavigationOptions
                {
                    ExpandedNodeIds = new HashSet<string>(StringComparer.Ordinal),
                    OnExpandedNodeIdsChanged = ids => reportedExpansion = ids,
                })
            .Node("plugins", "Plugins", plugins => plugins
                .MainView("all", "All", "pluginManager.home"))
            .Build();

        var plugins = Nodes(host)[0];
        host.SelectedNode = plugins;

        Assert.IsNotNull(reportedExpansion);
        CollectionAssert.Contains(reportedExpansion.ToArray(), "plugins");
        Assert.IsFalse(host.IsExpanded(plugins));
        AssertRow(plugins, "plugins", depth: 0, isExpanded: false, isSelected: false, containsActive: false);
    }

    [TestMethod]
    public void NavigationItemsOpenMainViewsAndExecuteCommandsThroughProvidedServices()
    {
        var workbench = new TestWorkbenchService(new WorkbenchViewState(null, null));
        string? executedCommandId = null;
        var host = SidebarNavigation.Create(
                workbench,
                new SidebarNavigationOptions
                {
                    ExecuteCommandAsync = (commandId, _) =>
                    {
                        executedCommandId = commandId;
                        return ValueTask.CompletedTask;
                    },
                })
            .MainView("home", "Home", "settings.home")
            .Command("reload", "Reload", "settings.reload")
            .Build();

        var nodes = Nodes(host);
        host.SelectedNode = nodes[0];
        nodes = Nodes(host);
        host.SelectedNode = nodes[1];

        CollectionAssert.AreEqual(new[] { "settings.home" }, workbench.OpenedMainViewIds.ToArray());
        Assert.AreEqual("settings.reload", executedCommandId);
    }

    private static IReadOnlyList<TreeViewNode> Nodes(TreeView treeView)
    {
        var nodes = (IReadOnlyList<TreeViewNode>?)treeView.Tag;
        Assert.IsNotNull(nodes);
        return nodes;
    }

    private static void AssertRow(
        TreeViewNode node,
        string nodeId,
        int depth,
        bool isExpanded,
        bool isSelected,
        bool containsActive)
    {
        Assert.IsInstanceOfType<SidebarNavigationRowState>(node.Tag);
        var state = (SidebarNavigationRowState)node.Tag!;
        Assert.AreEqual(nodeId, state.NodeId);
        Assert.AreEqual(depth, state.Depth);
        Assert.AreEqual(isExpanded, state.IsExpanded);
        Assert.AreEqual(isSelected, state.IsSelected);
        Assert.AreEqual(containsActive, state.ContainsActive);
    }

    private sealed class TestWorkbenchService(WorkbenchViewState current) : IWorkbenchService
    {
        public List<string> OpenedMainViewIds { get; } = [];

        public WorkbenchViewState Current { get; private set; } = current;

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
}
