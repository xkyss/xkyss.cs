using Mewoo.Abstractions.Views;
using Mewoo.Core.Contributions;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class MewooContributionRegistryScopeTests
{
    [TestMethod]
    public void BuildSnapshotInfersActivityScopeForSingleActivityPlugin()
    {
        var registry = new MewooContributionRegistry("xkyss.single");
        registry.Activity("xkyss.single.activity")
            .Title("Single")
            .ViewContainer("xkyss.single.views");
        registry.ViewContainer("xkyss.single.views")
            .Title("Single")
            .AddView("xkyss.single.sidebar", view => view
                .Title("Sidebar")
                .Create(_ => TestView("xkyss.single.sidebar")));
        registry.MainView("xkyss.single.home")
            .Title("Home")
            .Create(_ => TestView("xkyss.single.home"));
        registry.StatusBarItem("xkyss.single.status")
            .Text("Ready");

        var snapshot = registry.BuildSnapshot();

        Assert.AreEqual("xkyss.single.activity", snapshot.ViewContainers[0].ActivityScopeId);
        Assert.AreEqual("xkyss.single.activity", snapshot.ViewContainers[0].Views[0].ActivityScopeId);
        Assert.AreEqual("xkyss.single.activity", snapshot.MainViews[0].ActivityScopeId);
        Assert.IsFalse(snapshot.MainViews[0].IsGlobal);
        Assert.AreEqual("xkyss.single.activity", snapshot.StatusBarItems[0].ActivityScopeId);
        Assert.IsFalse(snapshot.StatusBarItems[0].IsGlobal);
    }

    [TestMethod]
    public void BuildSnapshotRejectsAmbiguousMainViewForMultiActivityPlugin()
    {
        var registry = new MewooContributionRegistry("xkyss.multi");
        registry.Activity("xkyss.multi.first")
            .Title("First")
            .ViewContainer("xkyss.multi.firstViews");
        registry.Activity("xkyss.multi.second")
            .Title("Second")
            .ViewContainer("xkyss.multi.secondViews");
        registry.ViewContainer("xkyss.multi.firstViews").Title("First");
        registry.ViewContainer("xkyss.multi.secondViews").Title("Second");
        registry.MainView("xkyss.multi.home")
            .Title("Home")
            .Create(_ => TestView("xkyss.multi.home"));

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => registry.BuildSnapshot());
        StringAssert.Contains(ex.Message, "requires an explicit Activity scope");
    }

    [TestMethod]
    public void BuildSnapshotUsesExplicitActivityScopeForMultiActivityPlugin()
    {
        var registry = new MewooContributionRegistry("xkyss.multi");
        registry.Activity("xkyss.multi.first")
            .Title("First")
            .ViewContainer("xkyss.multi.firstViews");
        registry.Activity("xkyss.multi.second")
            .Title("Second")
            .ViewContainer("xkyss.multi.secondViews");
        registry.ViewContainer("xkyss.multi.firstViews").Title("First");
        registry.ViewContainer("xkyss.multi.secondViews").Title("Second");
        registry.MainView("xkyss.multi.home")
            .ActivityScope("xkyss.multi.second")
            .Title("Home")
            .Create(_ => TestView("xkyss.multi.home"));
        registry.StatusBarItem("xkyss.multi.status")
            .ActivityScope("xkyss.multi.first")
            .Text("First");

        var snapshot = registry.BuildSnapshot();

        Assert.AreEqual("xkyss.multi.second", snapshot.MainViews[0].ActivityScopeId);
        Assert.IsFalse(snapshot.MainViews[0].IsGlobal);
        Assert.AreEqual("xkyss.multi.first", snapshot.StatusBarItems[0].ActivityScopeId);
        Assert.IsFalse(snapshot.StatusBarItems[0].IsGlobal);
    }

    [TestMethod]
    public void BuildSnapshotKeepsGlobalContributionScopeExplicit()
    {
        var registry = new MewooContributionRegistry("xkyss.global");
        registry.MainView("xkyss.global.logs")
            .Global()
            .Title("Logs")
            .Create(_ => TestView("xkyss.global.logs"));
        registry.StatusBarItem("xkyss.global.status")
            .Global()
            .Text("Global");

        var snapshot = registry.BuildSnapshot();

        Assert.IsTrue(snapshot.MainViews[0].IsGlobal);
        Assert.IsNull(snapshot.MainViews[0].ActivityScopeId);
        Assert.IsTrue(snapshot.StatusBarItems[0].IsGlobal);
        Assert.IsNull(snapshot.StatusBarItems[0].ActivityScopeId);
    }

    [TestMethod]
    public void BuildSnapshotRejectsImplicitScopeWithoutActivity()
    {
        var registry = new MewooContributionRegistry("xkyss.noActivity");
        registry.StatusBarItem("xkyss.noActivity.status").Text("No Activity");

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => registry.BuildSnapshot());
        StringAssert.Contains(ex.Message, "requires an Activity scope or explicit global scope");
    }

    private static MewooView TestView(string id) => new(id, new object());
}
