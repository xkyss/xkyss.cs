using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;
using Mewoo.Abstractions;
using Mewoo.Abstractions.Commands;
using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Views;
using Mewoo.Core.Plugins;

namespace Mewoo.Workbench;

public sealed class MewooWorkbenchWindow : MewooNativeWindow, IWorkbenchService
{
    private readonly MewooPluginHost _pluginHost;
    private readonly IServiceProvider _services;
    private readonly StackPanel _activityBar = new() { Orientation = Orientation.Vertical };
    private readonly Border _sidebarShell = new();
    private readonly Border _sidebarHost = new();
    private readonly StackPanel _tabBar = new() { Orientation = Orientation.Horizontal };
    private readonly Border _mainViewHost = new();
    private readonly Border _panelHost = new();
    private readonly StackPanel _statusLeft = new() { Orientation = Orientation.Horizontal };
    private readonly StackPanel _statusRight = new() { Orientation = Orientation.Horizontal };
    private readonly Dictionary<string, MainViewDescriptor> _mainViews;
    private readonly HashSet<string> _openMainViews = [];
    private bool _sidebarCollapsed;
    private bool _panelVisible;

    public MewooWorkbenchWindow(MewooPluginHost pluginHost, IServiceProvider services)
    {
        _pluginHost = pluginHost;
        _services = services;
        _mainViews = pluginHost.Contributions.MainViews.ToDictionary(x => x.Id, StringComparer.Ordinal);

        Title = "Mewoo";
        this.Resizable(1200, 760);
        this.StartCenterScreen();
        BuildTitleBar();
        Content = BuildWorkbench();
        RenderContributions();
        Loaded += async () =>
        {
            if (_pluginHost.Commands.Contains("quickLauncher.open"))
            {
                await _pluginHost.Commands.ExecuteAsync(
                    "quickLauncher.open",
                    new MewooCommandContext(_services, this));
            }
        };
    }

    public async ValueTask OpenMainViewAsync(string mainViewId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_mainViews.TryGetValue(mainViewId, out var descriptor))
        {
            throw new KeyNotFoundException($"Main view id '{mainViewId}' is not registered.");
        }

        if (!_openMainViews.Contains(mainViewId) || descriptor.CanOpenMultiple)
        {
            _tabBar.Children(CreateTabButton(descriptor));
            _openMainViews.Add(mainViewId);
        }

        Title = $"Mewoo - {descriptor.Title}";
        _mainViewHost.Child = HostView(descriptor.CreateView(new WorkbenchViewContext(descriptor.OwnerPluginId, _services, this)));
        await ValueTask.CompletedTask;
    }

    private void BuildTitleBar()
    {
        TitleBarLeft.Children(
            new TextBlock().Text("M").FontSize(16).SemiBold().CenterVertical().Margin(10, 0, 8, 0),
            new TextBlock().Text("Mewoo").SemiBold().CenterVertical(),
            new TextBlock().Text("0.1.0").FontSize(11).CenterVertical().Margin(6, 0, 12, 0),
            MenuText("File"),
            MenuText("View"),
            MenuText("Help"));

        TitleBarRight.Children(
            TextButton("Theme", "Theme"),
            TextButton("Side", "Toggle sidebar", ToggleSidebar),
            TextButton("Panel", "Toggle panel", TogglePanel),
            TextButton("Top", "Always on top", () => Topmost = !Topmost));
    }

    private FrameworkElement BuildWorkbench()
    {
        var statusBar = new Border()
            .MinHeight(24)
            .WithTheme((t, b) => b.Background(t.Palette.Accent))
            .Child(new DockPanel().Children(
                new Border().DockRight().Child(_statusRight),
                _statusLeft));

        _panelHost.MinHeight = 120;
        _panelHost.Height = 220;
        _panelHost.IsVisible = false;
        _panelHost.WithTheme((t, b) => b.Background(t.Palette.ControlBackground));
        _panelHost.Child = new DockPanel().Children(
            new TextBlock().DockTop().Text("Panel").SemiBold().Margin(12, 8),
            new TextBlock().Text("Logs and plugin output will appear here.").Margin(12));

        var mainArea = new DockPanel().Children(
            new Border()
                .DockTop()
                .MinHeight(35)
                .WithTheme((t, b) => b.Background(t.Palette.ControlBackground))
                .Child(_tabBar),
            _mainViewHost);

        _sidebarShell.DockLeft()
            .Width(260)
            .WithTheme((t, b) => b.Background(t.Palette.ControlBackground))
            .Child(_sidebarHost);

        var body = new DockPanel().Children(
            new Border()
                .DockLeft()
                .MinWidth(48)
                .Width(48)
                .WithTheme((t, b) => b.Background(t.Palette.WindowBackground))
                .Child(_activityBar),
            _sidebarShell,
            mainArea);

        return new DockPanel().Children(statusBar.DockBottom(), _panelHost.DockBottom(), body);
    }

    private void RenderContributions()
    {
        foreach (var activity in _pluginHost.Contributions.Activities.OrderBy(x => x.Order).ThenBy(x => x.Id))
        {
            _activityBar.Children(ActivityButton(activity));
        }

        foreach (var item in _pluginHost.Contributions.StatusBarItems)
        {
            var status = new TextBlock()
                .Text(item.Text)
                .FontSize(12)
                .CenterVertical()
                .Margin(8, 0);

            if (item.Alignment == StatusBarAlignment.Right)
            {
                _statusRight.Children(status);
            }
            else
            {
                _statusLeft.Children(status);
            }
        }

        var firstActivity = _pluginHost.Contributions.Activities.OrderBy(x => x.Order).FirstOrDefault();
        if (firstActivity is not null)
        {
            SelectActivity(firstActivity);
        }
    }

    private Button ActivityButton(ActivityDescriptor activity)
    {
        var button = new Button()
            .MinWidth(48)
            .MinHeight(48)
            .ToolTip(activity.Title)
            .Content(new TextBlock()
                .Text(activity.Icon ?? activity.Title[..1])
                .FontSize(16)
                .Center());

        button.Click += () => SelectActivity(activity);
        return button;
    }

    private void SelectActivity(ActivityDescriptor activity)
    {
        var container = _pluginHost.Contributions.ViewContainers.FirstOrDefault(x => x.Id == activity.ViewContainerId);
        if (container is null)
        {
            _sidebarHost.Child = ErrorBlock($"Missing view container: {activity.ViewContainerId}");
            return;
        }

        var views = new StackPanel { Orientation = Orientation.Vertical };
        views.Children(new TextBlock().Text(container.Title).SemiBold().Margin(12, 12, 12, 8));

        foreach (var view in container.Views)
        {
            views.Children(new TextBlock().Text(view.Title).Margin(12, 4));
            views.Children(HostView(view.CreateView(new WorkbenchViewContext(view.OwnerPluginId, _services, this))));
        }

        _sidebarHost.Child = views;
    }

    private UIElement HostView(IMewooView view)
    {
        return view.NativeView is UIElement element
            ? element
            : ErrorBlock($"View '{view.Id}' returned unsupported native view '{view.NativeView.GetType().FullName}'.");
    }

    private Button CreateTabButton(MainViewDescriptor descriptor)
    {
        var button = new Button()
            .Content(new TextBlock().Text(descriptor.Title).Margin(10, 0))
            .MinHeight(35);
        button.Click += async () => await OpenMainViewAsync(descriptor.Id);
        return button;
    }

    private static TextBlock MenuText(string text) => new TextBlock()
        .Text(text)
        .FontSize(12)
        .CenterVertical()
        .Margin(8, 0);

    private static Button IconButton(GlyphKind kind, string tooltip) => new Button()
        .ToolTip(tooltip)
        .MinWidth(34)
        .MinHeight(34)
        .Content(new GlyphElement().Kind(kind).GlyphSize(5));

    private void ToggleSidebar()
    {
        _sidebarCollapsed = !_sidebarCollapsed;
        _sidebarShell.IsVisible = !_sidebarCollapsed;
    }

    private void TogglePanel()
    {
        _panelVisible = !_panelVisible;
        _panelHost.IsVisible = _panelVisible;
    }

    private static Button TextButton(string text, string tooltip, Action? onClick = null)
    {
        var button = new Button()
        .ToolTip(tooltip)
        .MinWidth(48)
        .MinHeight(34)
        .Content(new TextBlock().Text(text).FontSize(12));
        if (onClick is not null)
        {
            button.Click += onClick;
        }

        return button;
    }

    private static TextBlock ErrorBlock(string message) => new TextBlock()
        .Text(message)
        .Margin(16)
        .Foreground(Color.FromRgb(232, 17, 35));

    private sealed record WorkbenchViewContext(string PluginId, IServiceProvider Services, IWorkbenchService Workbench) : IMewooViewContext;
}

public sealed record MewooCommandContext(IServiceProvider Services, IWorkbenchService Workbench) : IMewooCommandContext;
