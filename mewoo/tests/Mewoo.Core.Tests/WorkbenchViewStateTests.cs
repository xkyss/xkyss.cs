using Mewoo.Abstractions;
using Mewoo.Workbench;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class WorkbenchViewStateTests
{
    [TestMethod]
    public void CurrentViewStateTracksActiveActivityAndMainView()
    {
        var state = new WorkbenchState();

        state.OpenMainView("quickLauncher.home", "quickLauncher.activity");

        Assert.AreEqual(
            new WorkbenchViewState("quickLauncher.activity", "quickLauncher.home"),
            state.CurrentViewState);
    }

    [TestMethod]
    public void ViewStateChangedRaisesOnlyForActivityOrMainViewChanges()
    {
        var state = new WorkbenchState();
        var changes = new List<WorkbenchViewStateChangedEventArgs>();
        state.ViewStateChanged += (_, e) => changes.Add(e);

        state.ToggleSidebar();
        state.OpenMainView("quickLauncher.home", "quickLauncher.activity");
        state.TogglePanel();
        state.OpenMainView("quickLauncher.home", "quickLauncher.activity");
        state.OpenMainView("quickLauncher.details", "quickLauncher.activity");

        CollectionAssert.AreEqual(
            new[]
            {
                new WorkbenchViewState(null, null),
                new WorkbenchViewState("quickLauncher.activity", "quickLauncher.home"),
            },
            changes.Select(change => change.Previous).ToArray());
        CollectionAssert.AreEqual(
            new[]
            {
                new WorkbenchViewState("quickLauncher.activity", "quickLauncher.home"),
                new WorkbenchViewState("quickLauncher.activity", "quickLauncher.details"),
            },
            changes.Select(change => change.Current).ToArray());
    }
}
