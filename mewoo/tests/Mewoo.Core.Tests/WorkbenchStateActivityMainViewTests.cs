using Mewoo.Workbench;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class WorkbenchStateActivityMainViewTests
{
    [TestMethod]
    public void OpenMainViewStoresTabsPerActivity()
    {
        var state = new WorkbenchState();

        state.OpenMainView("quickLauncher.home", "quickLauncher.activity");
        state.OpenMainView("pluginManager.home", "pluginManager.activity");

        Assert.AreEqual("pluginManager.activity", state.ActiveActivityId);
        Assert.AreEqual("pluginManager.home", state.ActiveMainViewId);
        CollectionAssert.AreEqual(new[] { "pluginManager.home" }, state.OpenMainViewIds.ToArray());

        state.SetActiveActivity("quickLauncher.activity");

        Assert.AreEqual("quickLauncher.home", state.ActiveMainViewId);
        CollectionAssert.AreEqual(new[] { "quickLauncher.home" }, state.OpenMainViewIds.ToArray());
    }

    [TestMethod]
    public void RemoveMainViewRemovesAcrossAllActivityWorkspaces()
    {
        var state = new WorkbenchState();
        state.OpenMainView("shared.view", "first.activity");
        state.OpenMainView("first.only", "first.activity");
        state.OpenMainView("shared.view", "second.activity");

        state.RemoveMainView("shared.view");

        state.SetActiveActivity("first.activity");
        CollectionAssert.AreEqual(new[] { "first.only" }, state.OpenMainViewIds.ToArray());
        state.SetActiveActivity("second.activity");
        Assert.IsEmpty(state.OpenMainViewIds);
    }

    [TestMethod]
    public void RemoveMainViewsExceptCleansUnavailableViewsAcrossActivities()
    {
        var state = new WorkbenchState();
        state.OpenMainView("keep.view", "first.activity");
        state.OpenMainView("remove.view", "first.activity");
        state.OpenMainView("remove.view", "second.activity");

        state.RemoveMainViewsExcept(new HashSet<string>(["keep.view"], StringComparer.Ordinal));

        state.SetActiveActivity("first.activity");
        CollectionAssert.AreEqual(new[] { "keep.view" }, state.OpenMainViewIds.ToArray());
        Assert.AreEqual("keep.view", state.ActiveMainViewId);
        state.SetActiveActivity("second.activity");
        Assert.IsEmpty(state.OpenMainViewIds);
    }

    [TestMethod]
    public void SnapshotStoresAndRestoresPerActivityMainViewStacks()
    {
        var state = new WorkbenchState();
        state.OpenMainView("quickLauncher.home", "quickLauncher.activity");
        state.OpenMainView("pluginManager.home", "pluginManager.activity");
        state.OpenMainView("pluginManager.details", "pluginManager.activity");

        var snapshot = state.CreateSnapshot("dark", isAlwaysOnTop: true);
        var restored = new WorkbenchState();
        restored.Restore(snapshot, windowHeight: 760);

        Assert.AreEqual("pluginManager.activity", restored.ActiveActivityId);
        Assert.AreEqual("pluginManager.details", restored.ActiveMainViewId);
        CollectionAssert.AreEqual(
            new[] { "pluginManager.home", "pluginManager.details" },
            restored.OpenMainViewIds.ToArray());

        restored.SetActiveActivity("quickLauncher.activity");

        Assert.AreEqual("quickLauncher.home", restored.ActiveMainViewId);
        CollectionAssert.AreEqual(new[] { "quickLauncher.home" }, restored.OpenMainViewIds.ToArray());
        Assert.AreEqual(2, snapshot.ActivityMainViews?.Count);
    }

    [TestMethod]
    public void RestoreMigratesLegacyGlobalMainViewsIntoActiveActivity()
    {
        var snapshot = new WorkbenchStateSnapshot(
            SidebarCollapsed: false,
            SidebarWidth: WorkbenchState.SidebarDefaultWidth,
            PanelVisible: false,
            PanelHeight: 260,
            ActiveActivityId: "legacy.activity",
            ActiveMainViewId: "legacy.details",
            OpenMainViewIds: ["legacy.home", "legacy.details"],
            ThemeId: "dark",
            IsAlwaysOnTop: false);
        var restored = new WorkbenchState();

        restored.Restore(snapshot, windowHeight: 760);

        Assert.AreEqual("legacy.activity", restored.ActiveActivityId);
        Assert.AreEqual("legacy.details", restored.ActiveMainViewId);
        CollectionAssert.AreEqual(new[] { "legacy.home", "legacy.details" }, restored.OpenMainViewIds.ToArray());
    }
}
