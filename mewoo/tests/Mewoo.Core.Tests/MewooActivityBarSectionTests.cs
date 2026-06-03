using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Plugins;
using Mewoo.Abstractions.Views;
using Mewoo.Core.Contributions;
using Mewoo.Core.Plugins;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class MewooActivityBarSectionTests
{
    [TestMethod]
    public void ActivityDefaultsToPrimarySection()
    {
        var registry = new MewooContributionRegistry("xkyss.primary");
        registry.Activity("xkyss.primary.activity")
            .Title("Primary")
            .ViewContainer("xkyss.primary.views");

        var snapshot = registry.BuildSnapshot();

        Assert.AreEqual(ActivityBarSection.Primary, snapshot.Activities[0].Section);
    }

    [TestMethod]
    public void BuiltInRegistryCanDeclareSystemSection()
    {
        var registry = new MewooContributionRegistry("xkyss.system", allowSystemActivitySection: true);
        registry.Activity("xkyss.system.activity")
            .Title("System")
            .ViewContainer("xkyss.system.views")
            .ActivityBarSection(ActivityBarSection.System);

        var snapshot = registry.BuildSnapshot();

        Assert.AreEqual(ActivityBarSection.System, snapshot.Activities[0].Section);
    }

    [TestMethod]
    public void RuntimeRegistryRejectsSystemSection()
    {
        var registry = new MewooContributionRegistry("xkyss.runtime", allowSystemActivitySection: false);
        registry.Activity("xkyss.runtime.activity")
            .Title("Runtime")
            .ViewContainer("xkyss.runtime.views")
            .ActivityBarSection(ActivityBarSection.System);

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => registry.BuildSnapshot());
        StringAssert.Contains(ex.Message, "only built-in plugins can use the System section");
    }

    [TestMethod]
    public void PluginHostRejectsRuntimePluginSystemSection()
    {
        var host = new MewooPluginHost();

        var entry = host.RegisterPlugin(
            new RuntimeSystemSectionPlugin(),
            MewooPluginRegistrationSource.Runtime);

        Assert.AreEqual(MewooPluginState.Failed, entry.State);
        StringAssert.Contains(entry.Error?.Message, "only built-in plugins can use the System section");
        Assert.IsFalse(host.VisibleContributions.Activities.Any(activity =>
            activity.Id == "xkyss.runtimeSystem.activity"));
    }

    private sealed class RuntimeSystemSectionPlugin : IMewooPlugin
    {
        public string Id => "xkyss.runtimeSystem";

        public string DisplayName => "Runtime System";

        public void Register(IMewooContributionRegistry registry)
        {
            registry.Activity("xkyss.runtimeSystem.activity")
                .Title("Runtime System")
                .ViewContainer("xkyss.runtimeSystem.views")
                .ActivityBarSection(ActivityBarSection.System);
            registry.ViewContainer("xkyss.runtimeSystem.views")
                .Title("Runtime System")
                .AddView("xkyss.runtimeSystem.sidebar", view => view
                    .Title("Sidebar")
                    .Create(_ => new MewooView("xkyss.runtimeSystem.sidebar", new object())));
        }
    }
}
