using Mewoo.Abstractions;
using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Logging;
using Mewoo.Abstractions.Plugins;
using Mewoo.Abstractions.Views;
using Mewoo.Core.Plugins;

namespace Mewoo.Plugins.PluginManager;

public sealed class PluginManagerPlugin : IMewooPlugin
{
    private readonly PluginManagerDependencies _dependencies;
    private readonly PluginManagerSession _session = new();
    private readonly PluginManagerController _controller;

    public PluginManagerPlugin(
        MewooRuntimePluginManager runtimePlugins,
        MewooPluginHost pluginHost,
        IServiceProvider services,
        string pluginRoot,
        IMewooLogger? logger = null)
    {
        _dependencies = new PluginManagerDependencies(
            runtimePlugins,
            pluginHost,
            services,
            pluginRoot,
            logger);
        _controller = new PluginManagerController(_dependencies, _session);
    }

    public string Id => "pluginManager";

    public string DisplayName => "Plugin Manager";

    public void Register(IMewooContributionRegistry registry)
    {
        registry.Activity("pluginManager.activity")
            .Title("Plugins")
            .Icon("P")
            .ViewContainer("pluginManager.views")
            .ActivityBarSection(ActivityBarSection.System)
            .Order(80);

        registry.ViewContainer("pluginManager.views")
            .Title("Plugins")
            .AddView("pluginManager.installed", view => view
                .Title("Installed")
                .Create(ctx =>
                {
                    var views = CreateViews();
                    return new MewooView("pluginManager.installed", views.CreateSidebar(ctx.Workbench));
                }));

        registry.MainView("pluginManager.home")
            .Title("Plugins")
            .CanOpenMultiple(false)
            .Create(ctx =>
            {
                var views = CreateViews();
                return new MewooView("pluginManager.home", views.CreateMainView(ctx.Workbench));
            });

        registry.MainView("pluginManager.details")
            .Title("Plugin Details")
            .CanOpenMultiple(false)
            .Create(ctx =>
            {
                var views = CreateViews();
                return new MewooView("pluginManager.details", views.CreateDetailsView(ctx.Workbench));
            });

        registry.MainView("pluginManager.installPreview")
            .Title("Install Plugin")
            .CanOpenMultiple(false)
            .Create(ctx =>
            {
                var views = CreateViews();
                return new MewooView("pluginManager.installPreview", views.CreateInstallPreviewView(ctx.Workbench));
            });

        registry.MainView("pluginManager.updatePreview")
            .Title("Update Plugin")
            .CanOpenMultiple(false)
            .Create(ctx =>
            {
                var views = CreateViews();
                return new MewooView("pluginManager.updatePreview", views.CreateUpdatePreviewView(ctx.Workbench));
            });

        registry.MainView("pluginManager.runtimeStatus")
            .Title("Runtime Status")
            .CanOpenMultiple(false)
            .Create(ctx =>
            {
                var views = CreateViews();
                return new MewooView("pluginManager.runtimeStatus", views.Diagnostics.CreateRuntimeStatusView(ctx.Workbench));
            });

        registry.MainView("pluginManager.discoveryIssues")
            .Title("Discovery Issues")
            .CanOpenMultiple(false)
            .Create(ctx =>
            {
                var views = CreateViews();
                return new MewooView("pluginManager.discoveryIssues", views.Diagnostics.CreateDiscoveryIssuesView(ctx.Workbench));
            });

        registry.MainView("pluginManager.operationLog")
            .Title("Operation Log")
            .CanOpenMultiple(false)
            .Create(ctx =>
            {
                var views = CreateViews();
                return new MewooView("pluginManager.operationLog", views.Diagnostics.CreateOperationLogView(ctx.Workbench));
            });

        registry.Command("pluginManager.open")
            .Title("Open Plugin Manager")
            .Category("Plugins")
            .Execute(async (ctx, cancellationToken) =>
                await ctx.Workbench.OpenMainViewAsync("pluginManager.home", cancellationToken));
    }

    private PluginManagerViews CreateViews() =>
        new(_session, _controller);
}
