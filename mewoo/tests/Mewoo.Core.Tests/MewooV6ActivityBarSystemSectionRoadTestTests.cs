using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Plugins;
using Mewoo.Abstractions.Views;
using Mewoo.Core.Plugins;
using Mewoo.Workbench;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class MewooV6ActivityBarSystemSectionRoadTestTests
{
    private const string QuickLauncherActivity = "quickLauncher.activity";
    private const string RuntimeDiagnosticsActivity = "runtimeDiagnostics.activity";
    private const string PluginManagerActivity = "pluginManager.activity";
    private const string SettingsActivity = "settings.activity";

    [TestMethod]
    public void ActivityBarSystemSectionRoadTestCoversSectionsWorkspacesAndRuntimeRejection()
    {
        var sections = WorkbenchActivityBarModel.CreateSections(
            [
                Activity(PluginManagerActivity, ActivityBarSection.System, 80),
                Activity(SettingsActivity, ActivityBarSection.System, 90),
                Activity(QuickLauncherActivity, ActivityBarSection.Primary, 0),
                Activity(RuntimeDiagnosticsActivity, ActivityBarSection.Primary, 90),
            ]);

        CollectionAssert.AreEqual(
            new[] { QuickLauncherActivity, RuntimeDiagnosticsActivity },
            sections.Primary.Select(activity => activity.Id).ToArray());
        CollectionAssert.AreEqual(
            new[] { PluginManagerActivity, SettingsActivity },
            sections.System.Select(activity => activity.Id).ToArray());

        var state = new WorkbenchState();
        state.OpenMainView("quickLauncher.home", QuickLauncherActivity);
        state.OpenPanelTab(WorkbenchState.LogsPanelTabId);
        state.SetPanelHeight(320, 1000);

        state.OpenMainView("pluginManager.home", PluginManagerActivity);
        state.OpenMainView("settings.home", SettingsActivity);
        state.OpenPanelTab(WorkbenchState.LogsPanelTabId);
        state.SetPanelHeight(240, 1000);
        state.OpenMainView("runtimeDiagnostics.home", RuntimeDiagnosticsActivity);

        AssertWorkspace(
            state,
            RuntimeDiagnosticsActivity,
            expectedMainViews: ["runtimeDiagnostics.home"],
            expectedPanelVisible: false,
            expectedPanelHeight: WorkbenchState.PanelDefaultHeight,
            expectedPanelTabs: []);

        state.SetActiveActivity(PluginManagerActivity);
        AssertWorkspace(
            state,
            PluginManagerActivity,
            expectedMainViews: ["pluginManager.home"],
            expectedPanelVisible: false,
            expectedPanelHeight: WorkbenchState.PanelDefaultHeight,
            expectedPanelTabs: []);

        state.SetActiveActivity(SettingsActivity);
        AssertWorkspace(
            state,
            SettingsActivity,
            expectedMainViews: ["settings.home"],
            expectedPanelVisible: true,
            expectedPanelHeight: 240,
            expectedPanelTabs: [WorkbenchState.LogsPanelTabId]);

        state.SetActiveActivity(QuickLauncherActivity);
        AssertWorkspace(
            state,
            QuickLauncherActivity,
            expectedMainViews: ["quickLauncher.home"],
            expectedPanelVisible: true,
            expectedPanelHeight: 320,
            expectedPanelTabs: [WorkbenchState.LogsPanelTabId]);

        var host = new MewooPluginHost();
        var entry = host.RegisterPlugin(
            new RuntimeSystemSectionPlugin(),
            MewooPluginRegistrationSource.Runtime);

        Assert.AreEqual(MewooPluginState.Failed, entry.State);
        StringAssert.Contains(entry.Error?.Message, "only built-in plugins can use the System section");
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

    private sealed class RuntimeSystemSectionPlugin : IMewooPlugin
    {
        public string Id => "xkyss.v6RuntimeSystem";

        public string DisplayName => "V6 Runtime System";

        public void Register(IMewooContributionRegistry registry)
        {
            registry.Activity("xkyss.v6RuntimeSystem.activity")
                .Title("V6 Runtime System")
                .ViewContainer("xkyss.v6RuntimeSystem.views")
                .ActivityBarSection(ActivityBarSection.System);
            registry.ViewContainer("xkyss.v6RuntimeSystem.views")
                .Title("V6 Runtime System")
                .AddView("xkyss.v6RuntimeSystem.sidebar", view => view
                    .Title("Sidebar")
                    .Create(_ => new MewooView("xkyss.v6RuntimeSystem.sidebar", new object())));
        }
    }
}
