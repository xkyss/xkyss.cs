using Mewoo.Abstractions.Contributions;
using Mewoo.Workbench;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class MewooV5ActivityWorkspaceRoadTestTests
{
    private const string QuickLauncherActivity = "quickLauncher.activity";
    private const string PluginManagerActivity = "pluginManager.activity";
    private const string RuntimeDiagnosticsActivity = "runtimeDiagnostics.activity";

    [TestMethod]
    public void ActivityWorkspaceRoadTestCoversBuiltInPluginScopes()
    {
        var state = new WorkbenchState();
        var statusBar = new WorkbenchStatusBarModel();
        var statusItems = new[]
        {
            StatusItem("quickLauncher.status", QuickLauncherActivity, "QuickLauncher: ready"),
            StatusItem("workbench.global.status", activityScopeId: null, text: "Global: ready", isGlobal: true),
        };

        state.OpenMainView("quickLauncher.home", QuickLauncherActivity);
        state.OpenPanelTab(WorkbenchState.LogsPanelTabId);
        state.SetPanelHeight(340, 1000);
        statusBar.UpdateText("quickLauncher.status", "QuickLauncher: 3 items");

        AssertWorkspace(
            state,
            QuickLauncherActivity,
            expectedMainViews: ["quickLauncher.home"],
            expectedPanelVisible: true,
            expectedPanelHeight: 340,
            expectedPanelTabs: [WorkbenchState.LogsPanelTabId]);
        CollectionAssert.AreEqual(
            new[] { "quickLauncher.status", "workbench.global.status" },
            statusBar.GetVisibleItems(statusItems, state.ActiveActivityId).Select(item => item.Descriptor.Id).ToArray());

        state.OpenMainView("pluginManager.home", PluginManagerActivity);
        state.OpenMainView("pluginManager.details", PluginManagerActivity);

        AssertWorkspace(
            state,
            PluginManagerActivity,
            expectedMainViews: ["pluginManager.home", "pluginManager.details"],
            expectedPanelVisible: false,
            expectedPanelHeight: WorkbenchState.PanelDefaultHeight,
            expectedPanelTabs: []);
        CollectionAssert.AreEqual(
            new[] { "workbench.global.status" },
            statusBar.GetVisibleItems(statusItems, state.ActiveActivityId).Select(item => item.Descriptor.Id).ToArray());

        state.OpenPanelTab(WorkbenchState.LogsPanelTabId);
        state.SetPanelHeight(220, 1000);
        state.OpenMainView("runtimeDiagnostics.home", RuntimeDiagnosticsActivity);

        AssertWorkspace(
            state,
            RuntimeDiagnosticsActivity,
            expectedMainViews: ["runtimeDiagnostics.home"],
            expectedPanelVisible: false,
            expectedPanelHeight: WorkbenchState.PanelDefaultHeight,
            expectedPanelTabs: []);
        CollectionAssert.AreEqual(
            new[] { "workbench.global.status" },
            statusBar.GetVisibleItems(statusItems, state.ActiveActivityId).Select(item => item.Descriptor.Id).ToArray());

        state.OpenPanelTab(WorkbenchState.LogsPanelTabId);
        state.SetActiveActivity(QuickLauncherActivity);

        AssertWorkspace(
            state,
            QuickLauncherActivity,
            expectedMainViews: ["quickLauncher.home"],
            expectedPanelVisible: true,
            expectedPanelHeight: 340,
            expectedPanelTabs: [WorkbenchState.LogsPanelTabId]);
        Assert.AreEqual(
            "QuickLauncher: 3 items",
            statusBar.GetVisibleItems(statusItems, state.ActiveActivityId)
                .Single(item => item.Descriptor.Id == "quickLauncher.status")
                .Text);

        state.SetActiveActivity(PluginManagerActivity);

        AssertWorkspace(
            state,
            PluginManagerActivity,
            expectedMainViews: ["pluginManager.home", "pluginManager.details"],
            expectedPanelVisible: true,
            expectedPanelHeight: 220,
            expectedPanelTabs: [WorkbenchState.LogsPanelTabId]);
    }

    private static void AssertWorkspace(
        WorkbenchState state,
        string expectedActivity,
        string[] expectedMainViews,
        bool expectedPanelVisible,
        double expectedPanelHeight,
        string[] expectedPanelTabs)
    {
        Assert.AreEqual(expectedActivity, state.ActiveActivityId);
        CollectionAssert.AreEqual(expectedMainViews, state.OpenMainViewIds.ToArray());
        Assert.AreEqual(expectedPanelVisible, state.PanelVisible);
        Assert.AreEqual(expectedPanelHeight, state.PanelHeight);
        CollectionAssert.AreEqual(expectedPanelTabs, state.OpenPanelTabIds.ToArray());
    }

    private static StatusBarItemDescriptor StatusItem(
        string id,
        string? activityScopeId,
        string text,
        bool isGlobal = false)
    {
        return new StatusBarItemDescriptor(
            id,
            "road-test",
            isGlobal ? null : activityScopeId,
            isGlobal,
            StatusBarAlignment.Left,
            text,
            CommandId: null);
    }
}
