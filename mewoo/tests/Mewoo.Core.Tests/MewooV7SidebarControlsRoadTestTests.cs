using Aprillz.MewUI.Controls;
using Mewoo.Abstractions;
using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Views;
using Mewoo.Controls.Sidebar;
using Mewoo.Workbench;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class MewooV7SidebarControlsRoadTestTests
{
    [TestMethod]
    public void SidebarControlsRoadTestCoversPluginOwnedChromeAndNavigationBehavior()
    {
        var container = new ViewContainerDescriptor(
            "settings.views",
            "settings",
            "settings.activity",
            "Settings Metadata",
            [SidebarView("settings.general", "General Metadata")]);
        var renderedSidebar = WorkbenchSidebarContentRenderer.Render(
            container,
            view => new Border { Tag = view.Title });

        Assert.AreEqual(1, renderedSidebar.Count);
        Assert.IsInstanceOfType<Border>(renderedSidebar[0]);

        var header = (Grid)SidebarHeader.Create("Very Long Settings Title")
            .Action("A", "Appearance", () => { })
            .Action("P", "Plugins", () => { })
            .Action("R", "Reload", () => { })
            .More(menu => menu.Item("Open Settings", () => { }))
            .Build();
        CollectionAssert.AreEqual(
            new[] { 0, 1, 2 },
            header.Children.Select(Grid.GetColumn).ToArray());
        Assert.AreEqual(3, ((StackPanel)header[1]).Count);

        var search = SidebarSearchRow.Create("Search settings");
        Assert.AreEqual("Search settings", search.Placeholder);

        var workbench = new TestWorkbenchService(
            new WorkbenchViewState("settings.activity", "settings.home"));
        IReadOnlySet<string>? controlledExpansion = null;
        var navigation = SidebarNavigation.Create(
                workbench,
                new SidebarNavigationOptions
                {
                    ExpandedNodeIds = new HashSet<string>(["settings"], StringComparer.Ordinal),
                    OnExpandedNodeIdsChanged = ids => controlledExpansion = ids,
                    ExecuteCommandAsync = (_, _) => ValueTask.CompletedTask,
                })
            .Node("settings", "Settings", settings => settings
                .MainView("general", "General", "settings.home")
                .Node("advanced", "Advanced", advanced => advanced
                    .Command("reload", "Reload", "settings.reload")))
            .MainView("plugins", "Plugins", "pluginManager.home")
            .Build();

        var settings = Nodes(navigation)[0];
        var general = settings.Children[0];
        var advanced = settings.Children[1];
        var plugins = Nodes(navigation)[1];

        Assert.IsTrue(navigation.IsExpanded(settings));
        Assert.AreSame(general, navigation.SelectedNode);
        AssertRow(settings, "settings", depth: 0, isExpanded: true, isSelected: false, containsActive: true);
        AssertRow(general, "general", depth: 1, isExpanded: false, isSelected: true, containsActive: false);
        AssertRow(advanced, "advanced", depth: 1, isExpanded: false, isSelected: false, containsActive: false);

        navigation.SelectedNode = plugins;

        CollectionAssert.AreEqual(new[] { "pluginManager.home" }, workbench.OpenedMainViewIds.ToArray());
        settings = Nodes(navigation)[0];
        plugins = Nodes(navigation)[1];
        AssertRow(settings, "settings", depth: 0, isExpanded: true, isSelected: false, containsActive: false);
        AssertRow(plugins, "plugins", depth: 0, isExpanded: false, isSelected: true, containsActive: false);

        navigation.SelectedNode = settings;
        Assert.IsNotNull(controlledExpansion);
        CollectionAssert.DoesNotContain(controlledExpansion.ToArray(), "settings");
    }

    private static SidebarViewDescriptor SidebarView(string id, string title)
    {
        return new SidebarViewDescriptor(
            id,
            "settings",
            "settings.activity",
            title,
            _ => new MewooView(id, new object()));
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
