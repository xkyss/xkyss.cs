using Mewoo.Abstractions.Contributions;
using Mewoo.Workbench;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class WorkbenchStatusBarModelTests
{
    [TestMethod]
    public void VisibleItemsIncludeActiveActivityScopeAndGlobalItems()
    {
        var model = new WorkbenchStatusBarModel();
        var visible = model.GetVisibleItems(
            [
                Item("smoke.status", activityScopeId: "smoke"),
                Item("runtime.status", activityScopeId: "runtime"),
                Item("global.status", isGlobal: true),
            ],
            activeActivityId: "smoke");

        CollectionAssert.AreEqual(
            new[] { "smoke.status", "global.status" },
            visible.Select(item => item.Descriptor.Id).ToArray());
    }

    [TestMethod]
    public void SwitchingActivityChangesActivityScopedItems()
    {
        var model = new WorkbenchStatusBarModel();
        var items = new[]
        {
            Item("smoke.status", activityScopeId: "smoke"),
            Item("runtime.status", activityScopeId: "runtime"),
            Item("global.status", isGlobal: true),
        };

        var runtimeVisible = model.GetVisibleItems(items, activeActivityId: "runtime");

        CollectionAssert.AreEqual(
            new[] { "runtime.status", "global.status" },
            runtimeVisible.Select(item => item.Descriptor.Id).ToArray());
    }

    [TestMethod]
    public void RuntimeUpdatesAreRetainedForActivityScopedAndGlobalItems()
    {
        var model = new WorkbenchStatusBarModel();
        var items = new[]
        {
            Item("smoke.status", activityScopeId: "smoke", text: "Smoke: idle"),
            Item("runtime.status", activityScopeId: "runtime", text: "Runtime: idle"),
            Item("global.status", isGlobal: true, text: "Global: idle"),
        };

        model.UpdateText("runtime.status", "Runtime: loaded");
        model.UpdateText("global.status", "Global: ready");

        var smokeVisible = model.GetVisibleItems(items, activeActivityId: "smoke");

        Assert.AreEqual("Smoke: idle", smokeVisible.Single(item => item.Descriptor.Id == "smoke.status").Text);
        Assert.AreEqual("Global: ready", smokeVisible.Single(item => item.Descriptor.Id == "global.status").Text);

        var runtimeVisible = model.GetVisibleItems(items, activeActivityId: "runtime");

        Assert.AreEqual("Runtime: loaded", runtimeVisible.Single(item => item.Descriptor.Id == "runtime.status").Text);
        Assert.AreEqual("Global: ready", runtimeVisible.Single(item => item.Descriptor.Id == "global.status").Text);
    }

    [TestMethod]
    public void MissingRuntimeUpdateIsPruned()
    {
        var model = new WorkbenchStatusBarModel();
        model.UpdateText("removed.status", "removed");
        model.RemoveMissing(new HashSet<string>(StringComparer.Ordinal) { "smoke.status" });

        var visible = model.GetVisibleItems(
            [Item("removed.status", activityScopeId: "smoke", text: "default")],
            activeActivityId: "smoke");

        Assert.AreEqual("default", visible.Single().Text);
    }

    private static StatusBarItemDescriptor Item(
        string id,
        string? activityScopeId = null,
        bool isGlobal = false,
        string text = "ready")
    {
        return new StatusBarItemDescriptor(
            id,
            "test.plugin",
            isGlobal ? null : activityScopeId,
            isGlobal,
            StatusBarAlignment.Left,
            text,
            CommandId: null);
    }
}
