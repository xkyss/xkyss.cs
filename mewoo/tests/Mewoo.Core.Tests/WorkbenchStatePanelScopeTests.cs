using Mewoo.Workbench;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class WorkbenchStatePanelScopeTests
{
    [TestMethod]
    public void PanelVisibilityAndHeightAreScopedToActiveActivity()
    {
        var state = new WorkbenchState();

        state.SetActiveActivity("smoke");
        state.TogglePanel();
        state.SetPanelHeight(360, 1000);

        Assert.IsTrue(state.PanelVisible);
        Assert.AreEqual(360, state.PanelHeight);

        state.SetActiveActivity("runtime");

        Assert.IsFalse(state.PanelVisible);
        Assert.AreEqual(WorkbenchState.PanelDefaultHeight, state.PanelHeight);

        state.TogglePanel();
        state.SetPanelHeight(180, 1000);

        state.SetActiveActivity("smoke");

        Assert.IsTrue(state.PanelVisible);
        Assert.AreEqual(360, state.PanelHeight);
    }

    [TestMethod]
    public void ActivityScopedPanelTabsDoNotAppearInOtherActivities()
    {
        var state = new WorkbenchState();

        state.SetActiveActivity("smoke");
        state.OpenPanelTab("smoke.problems");

        Assert.AreEqual("smoke.problems", state.ActivePanelTabId);
        CollectionAssert.Contains(state.OpenPanelTabIds.ToArray(), "smoke.problems");

        state.SetActiveActivity("runtime");

        Assert.IsNull(state.ActivePanelTabId);
        CollectionAssert.DoesNotContain(state.OpenPanelTabIds.ToArray(), "smoke.problems");

        state.OpenPanelTab("runtime.tasks");
        state.SetActiveActivity("smoke");

        Assert.AreEqual("smoke.problems", state.ActivePanelTabId);
        CollectionAssert.DoesNotContain(state.OpenPanelTabIds.ToArray(), "runtime.tasks");
    }

    [TestMethod]
    public void LogsCanOpenFromAnyActivityWithoutGlobalPanelVisibility()
    {
        var state = new WorkbenchState();

        state.SetActiveActivity("smoke");
        state.OpenPanelTab(WorkbenchState.LogsPanelTabId);

        Assert.IsTrue(state.PanelVisible);
        CollectionAssert.Contains(state.OpenPanelTabIds.ToArray(), WorkbenchState.LogsPanelTabId);

        state.SetActiveActivity("runtime");

        Assert.IsFalse(state.PanelVisible);
        CollectionAssert.DoesNotContain(state.OpenPanelTabIds.ToArray(), WorkbenchState.LogsPanelTabId);

        state.OpenPanelTab(WorkbenchState.LogsPanelTabId);

        Assert.IsTrue(state.PanelVisible);
        Assert.AreEqual(WorkbenchState.LogsPanelTabId, state.ActivePanelTabId);

        state.SetActiveActivity("smoke");

        Assert.IsTrue(state.PanelVisible);
        Assert.AreEqual(WorkbenchState.LogsPanelTabId, state.ActivePanelTabId);
    }

    [TestMethod]
    public void RemovePanelTabsExceptPrunesUnavailableTabsPerActivity()
    {
        var state = new WorkbenchState();

        state.SetActiveActivity("smoke");
        state.OpenPanelTab("smoke.problems");
        state.SetActiveActivity("runtime");
        state.OpenPanelTab("runtime.tasks");
        state.OpenPanelTab(WorkbenchState.LogsPanelTabId);

        state.RemovePanelTabsExcept(new HashSet<string>(StringComparer.Ordinal)
        {
            WorkbenchState.LogsPanelTabId,
        });

        state.SetActiveActivity("smoke");
        Assert.AreEqual(0, state.OpenPanelTabIds.Count);
        Assert.IsNull(state.ActivePanelTabId);

        state.SetActiveActivity("runtime");
        CollectionAssert.AreEqual(new[] { WorkbenchState.LogsPanelTabId }, state.OpenPanelTabIds.ToArray());
        Assert.AreEqual(WorkbenchState.LogsPanelTabId, state.ActivePanelTabId);
    }

    [TestMethod]
    public void SnapshotRestoresActivityPanelState()
    {
        var state = new WorkbenchState();
        state.SetActiveActivity("smoke");
        state.OpenPanelTab("smoke.problems");
        state.SetPanelHeight(420, 1000);
        state.SetActiveActivity("runtime");
        state.OpenPanelTab(WorkbenchState.LogsPanelTabId);

        var restored = new WorkbenchState();
        restored.Restore(state.CreateSnapshot("dark", isAlwaysOnTop: false), 1000);

        Assert.AreEqual("runtime", restored.ActiveActivityId);
        Assert.IsTrue(restored.PanelVisible);
        Assert.AreEqual(WorkbenchState.LogsPanelTabId, restored.ActivePanelTabId);

        restored.SetActiveActivity("smoke");

        Assert.IsTrue(restored.PanelVisible);
        Assert.AreEqual(420, restored.PanelHeight);
        Assert.AreEqual("smoke.problems", restored.ActivePanelTabId);
    }

    [TestMethod]
    public void LegacySnapshotMigratesPanelStateToActiveActivity()
    {
        var snapshot = new WorkbenchStateSnapshot(
            SidebarCollapsed: false,
            SidebarWidth: WorkbenchState.SidebarDefaultWidth,
            PanelVisible: true,
            PanelHeight: 380,
            ActiveActivityId: "smoke",
            ActiveMainViewId: null,
            OpenMainViewIds: [],
            ThemeId: "dark",
            IsAlwaysOnTop: false);

        var state = new WorkbenchState();
        state.Restore(snapshot, 1000);

        Assert.AreEqual("smoke", state.ActiveActivityId);
        Assert.IsTrue(state.PanelVisible);
        Assert.AreEqual(380, state.PanelHeight);
        Assert.AreEqual(WorkbenchState.LogsPanelTabId, state.ActivePanelTabId);
        CollectionAssert.AreEqual(new[] { WorkbenchState.LogsPanelTabId }, state.OpenPanelTabIds.ToArray());
    }
}
