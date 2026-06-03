using Mewoo.Abstractions.Contributions;
using Mewoo.Workbench;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class WorkbenchActivityBarModelTests
{
    [TestMethod]
    public void CreateSectionsSplitsPrimaryAndSystemActivities()
    {
        var sections = WorkbenchActivityBarModel.CreateSections(
            [
                Activity("pluginManager.activity", ActivityBarSection.System, 80),
                Activity("quickLauncher.activity", ActivityBarSection.Primary, 0),
                Activity("settings.activity", ActivityBarSection.System, 90),
                Activity("runtimeDiagnostics.activity", ActivityBarSection.Primary, 90),
            ]);

        CollectionAssert.AreEqual(
            new[] { "quickLauncher.activity", "runtimeDiagnostics.activity" },
            sections.Primary.Select(activity => activity.Id).ToArray());
        CollectionAssert.AreEqual(
            new[] { "pluginManager.activity", "settings.activity" },
            sections.System.Select(activity => activity.Id).ToArray());
    }

    [TestMethod]
    public void CreateSectionsSortsEachSectionByOrderThenId()
    {
        var sections = WorkbenchActivityBarModel.CreateSections(
            [
                Activity("settings.activity", ActivityBarSection.System, 90),
                Activity("pluginManager.activity", ActivityBarSection.System, 80),
                Activity("runtimeDiagnostics.activity", ActivityBarSection.Primary, 90),
                Activity("quickLauncher.activity", ActivityBarSection.Primary, 0),
                Activity("alpha.activity", ActivityBarSection.Primary, 0),
            ]);

        CollectionAssert.AreEqual(
            new[] { "alpha.activity", "quickLauncher.activity", "runtimeDiagnostics.activity" },
            sections.Primary.Select(activity => activity.Id).ToArray());
        CollectionAssert.AreEqual(
            new[] { "pluginManager.activity", "settings.activity" },
            sections.System.Select(activity => activity.Id).ToArray());
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
}
