using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;
using Mewoo.Abstractions;
using Mewoo.Abstractions.Commands;
using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Logging;
using Mewoo.Abstractions.Storage;
using Mewoo.Abstractions.Views;
using Mewoo.Core.Plugins;

namespace Mewoo.Workbench;

public sealed class MewooWorkbenchWindow : MewooNativeWindow, IWorkbenchService
{
    private static readonly PathGeometry LightThemeIcon = PathGeometry.Parse(
        @"M8.462,15.537C7.487,14.563,7,13.383,7,12c0-1.383,0.487-2.563,1.462-3.538S10.617,7,12,7
            c1.383,0,2.563,0.487,3.537,1.462C16.513,9.438,17,10.617,17,12c0,1.383-0.487,2.563-1.463,3.537C14.563,16.513,13.383,17,12,17
            C10.617,17,9.438,16.513,8.462,15.537z M5,13H1v-2h4V13z M23,13h-4v-2h4V13z M11,5V1h2v4H11z M11,23v-4h2v4H11z M6.4,7.75
            L3.875,5.325L5.3,3.85l2.4,2.5L6.4,7.75z M18.7,20.15l-2.425-2.525L17.6,16.25l2.525,2.425L18.7,20.15z M16.25,6.4l2.425-2.525
            L20.15,5.3l-2.5,2.4L16.25,6.4z M3.85,18.7l2.525-2.425L7.75,17.6l-2.425,2.525L3.85,18.7z");

    private static readonly PathGeometry DarkThemeIcon = PathGeometry.Parse(
        @"M12.058,19.904c-2.222,0-4.111-0.777-5.667-2.334c-1.556-1.555-2.333-3.444-2.333-5.667
            c0-2.025,0.66-3.782,1.981-5.27C7.359,5.147,8.994,4.269,10.942,4c0.054,0,0.106,0.002,0.159,0.006
            c0.052,0.004,0.103,0.009,0.153,0.017c-0.337,0.471-0.604,0.994-0.801,1.57s-0.295,1.18-0.295,1.811
            c0,1.778,0.622,3.289,1.867,4.533c1.244,1.245,2.755,1.867,4.533,1.867c0.635,0,1.239-0.099,1.813-0.296
            c0.574-0.195,1.09-0.463,1.549-0.801c0.007,0.051,0.013,0.102,0.017,0.154c0.004,0.051,0.006,0.104,0.006,0.158
            c-0.257,1.949-1.128,3.583-2.615,4.904C15.84,19.244,14.084,19.904,12.058,19.904z M12.058,18.904c1.467,0,2.784-0.404,3.95-1.213
            s2.017-1.863,2.55-3.162c-0.333,0.083-0.667,0.149-1,0.199c-0.333,0.051-0.667,0.075-1,0.075c-2.05,0-3.796-0.721-5.237-2.163
            C9.878,11.2,9.158,9.454,9.158,7.404c0-0.333,0.025-0.667,0.075-1c0.05-0.333,0.117-0.667,0.2-1c-1.3,0.533-2.354,1.383-3.163,2.55
            c-0.808,1.167-1.212,2.483-1.212,3.95c0,1.934,0.684,3.583,2.05,4.95C8.475,18.221,10.125,18.904,12.058,18.904z");

    private static readonly PathGeometry SidebarIcon = PathGeometry.Parse(
        "M3,5h18v2H3z M3,17h18v2H3z M3,5h2v14H3z M19,5h2v14h-2z M8,5h2v14H8z");

    private static readonly PathGeometry PanelIcon = PathGeometry.Parse(
        "M3,5h18v2H3z M3,17h18v2H3z M3,5h2v14H3z M19,5h2v14h-2z M3,13h18v2H3z");

    private static readonly PathGeometry TopmostIcon = PathGeometry.Parse(
        "M12,3l6,6h-4v8h-4V9H6z M5,19h14v2H5z");

    private readonly MewooPluginHost _pluginHost;
    private readonly IServiceProvider _services;
    private readonly MewooThemeController _themeController;
    private readonly IStateStorage? _stateStorage;
    private readonly IMewooLogger _logger;
    private readonly SemaphoreSlim _stateSaveLock = new(1, 1);
    private readonly WorkbenchState _state = new();
    private readonly StackPanel _activityBar = new() { Orientation = Orientation.Vertical };
    private readonly Border _sidebarShell = new();
    private readonly Border _sidebarHost = new();
    private readonly ClosableTabControl _mainTabs = new();
    private readonly Border _panelHost = new();
    private readonly StackPanel _logsPanel = new() { Orientation = Orientation.Vertical };
    private readonly StackPanel _statusLeft = new() { Orientation = Orientation.Horizontal };
    private readonly StackPanel _statusRight = new() { Orientation = Orientation.Horizontal };
    private readonly Dictionary<string, TextBlock> _statusTextById = new(StringComparer.Ordinal);
    private readonly Dictionary<string, MainViewDescriptor> _mainViews;
    private bool _isRestoringState;
    private bool _isResizingSidebar;

    public MewooWorkbenchWindow(
        MewooPluginHost pluginHost,
        IServiceProvider services,
        MewooThemeController? themeController = null,
        IStateStorage? stateStorage = null,
        IMewooLogger? logger = null)
    {
        _pluginHost = pluginHost;
        _services = services;
        _themeController = themeController ?? new MewooThemeController();
        _stateStorage = stateStorage;
        _logger = logger ?? new NullMewooLogger();
        _mainViews = pluginHost.VisibleContributions.MainViews.ToDictionary(x => x.Id, StringComparer.Ordinal);
        _state.Changed += OnWorkbenchStateChanged;
        _pluginHost.ContributionsChanged += RenderContributions;
        _themeController.Changed += OnThemeChanged;
        _logger.Changed += RenderLogs;

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

        if (!_state.OpenMainViewIds.Contains(mainViewId, StringComparer.Ordinal) || descriptor.CanOpenMultiple)
        {
            AddMainTab(descriptor);
            _mainTabs.SelectLast();
        }
        else
        {
            var index = _state.OpenMainViewIds.ToList().FindIndex(id => string.Equals(id, mainViewId, StringComparison.Ordinal));
            _mainTabs.Select(index);
        }

        Title = $"Mewoo - {descriptor.Title}";
        _state.OpenMainView(mainViewId);
        await ValueTask.CompletedTask;
    }

    public void UpdateStatusBarItem(string statusBarItemId, string text)
    {
        if (_statusTextById.TryGetValue(statusBarItemId, out var status))
        {
            status.Text = text;
        }
    }

    public void OpenLogsPanel()
    {
        ShowLogs();
    }

    public async ValueTask RestoreStateAsync(WorkbenchStateSnapshot? snapshot, CancellationToken cancellationToken = default)
    {
        if (snapshot is null)
        {
            _themeController.Apply(_themeController.CurrentTheme.Id);
            return;
        }

        _isRestoringState = true;
        try
        {
            Topmost = snapshot.IsAlwaysOnTop;
            _themeController.Apply(snapshot.ThemeId);
            _state.Restore(snapshot, ClientSize.Height);
            ApplyState();
            RenderContributions();

            foreach (var mainViewId in snapshot.OpenMainViewIds)
            {
                if (string.Equals(mainViewId, snapshot.ActiveMainViewId, StringComparison.Ordinal))
                {
                    continue;
                }

                if (_mainViews.ContainsKey(mainViewId))
                {
                    await OpenMainViewAsync(mainViewId, cancellationToken);
                }
            }

            if (snapshot.ActiveMainViewId is not null && _mainViews.ContainsKey(snapshot.ActiveMainViewId))
            {
                await OpenMainViewAsync(snapshot.ActiveMainViewId, cancellationToken);
            }
        }
        finally
        {
            _isRestoringState = false;
        }
    }

    private void BuildTitleBar()
    {
        TitleBarLeft.Children(
            new TextBlock().Text("M").FontSize(16).SemiBold().CenterVertical().Margin(10, 0, 8, 0),
            MenuText("File"),
            MenuText("View"),
            MenuText("Help"));

        TitleBarRight.Children(
            TitleChromeButton(ThemeIcon(), "Toggle Dark/Light theme", _themeController.Toggle),
            TitleChromeButton(PathIcon(SidebarIcon, 14), "Toggle sidebar", ToggleSidebar),
            TitleChromeButton(PathIcon(PanelIcon, 14), "Toggle panel", TogglePanel),
            TitleChromeButton(PathIcon(TopmostIcon, 14), "Always on top", ToggleAlwaysOnTop));
    }

    private FrameworkElement BuildWorkbench()
    {
        var statusBar = new Border()
            .MinHeight(24)
            .WithTheme((t, b) => b.Background(t.Palette.Accent))
            .Child(new DockPanel().Children(
                new Border().DockRight().Child(_statusRight),
                _statusLeft));

        var logsTab = new DockPanel().Children(
            ResizeGrip.Create(ResizeGripOrientation.Horizontal, delta =>
            {
                _state.SetPanelHeight(_state.PanelHeight + delta, ClientSize.Height);
            }).DockTop(),
            _logsPanel);

        var panelTabs = new TabControl
        {
            VerticalScroll = ScrollMode.Disabled,
            HorizontalScroll = ScrollMode.Disabled,
        };
        panelTabs.AddTab(new TabItem { Header = new Label { Text = "Logs" }, Content = logsTab });

        _panelHost.MinHeight = WorkbenchState.PanelMinHeight;
        _panelHost.Height = _state.PanelHeight;
        _panelHost.IsVisible = _state.PanelVisible;
        _panelHost.WithTheme((t, b) => b.Background(t.Palette.ControlBackground));
        _panelHost.Child = panelTabs;
        RenderLogs();

        var mainArea = _mainTabs.Inner;
        mainArea.MinWidth = WorkbenchState.MainAreaMinWidth;

        _sidebarShell.DockLeft()
            .Width(_state.SidebarWidth)
            .WithTheme((t, b) => b.Background(t.Palette.ControlBackground))
            .Child(new DockPanel().Children(
                ResizeGrip.CreateVerticalAbsolute(
                    () => _state.SidebarWidth,
                    width =>
                {
                    _isResizingSidebar = true;
                    _state.ResizeSidebar(width, ClientSize.Width);
                },
                    () =>
                {
                    _isResizingSidebar = false;
                    ApplyState();
                }).DockRight(),
                _sidebarHost));

        var mainColumn = new DockPanel
        {
            MinWidth = WorkbenchState.MainAreaMinWidth,
        }.Children(
            _panelHost.DockBottom(),
            mainArea);

        var body = new DockPanel().Children(
            new Border()
                .DockLeft()
                .MinWidth(WorkbenchState.ActivityBarWidth)
                .Width(WorkbenchState.ActivityBarWidth)
                .WithTheme((t, b) => b.Background(t.Palette.WindowBackground))
                .Child(_activityBar),
            _sidebarShell,
            mainColumn);

        return new DockPanel().Children(statusBar.DockBottom(), body);
    }

    private void RenderContributions()
    {
        _activityBar.Clear();
        _mainTabs.Clear();
        _statusLeft.Clear();
        _statusRight.Clear();
        _statusTextById.Clear();
        _mainViews.Clear();

        foreach (var mainView in _pluginHost.VisibleContributions.MainViews)
        {
            _mainViews[mainView.Id] = mainView;
        }

        foreach (var openMainViewId in _state.OpenMainViewIds.ToArray())
        {
            if (!_mainViews.ContainsKey(openMainViewId))
            {
                _state.RemoveMainView(openMainViewId);
            }
        }

        foreach (var openMainViewId in _state.OpenMainViewIds)
        {
            if (_mainViews.TryGetValue(openMainViewId, out var descriptor))
            {
                AddMainTab(descriptor);
            }
        }

        foreach (var activity in _pluginHost.VisibleContributions.Activities.OrderBy(x => x.Order).ThenBy(x => x.Id))
        {
            _activityBar.Children(ActivityButton(activity));
        }

        foreach (var item in _pluginHost.VisibleContributions.StatusBarItems)
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

            _statusTextById[item.Id] = status;
        }

        RenderThemeStatus();

        var activities = _pluginHost.VisibleContributions.Activities.OrderBy(x => x.Order).ThenBy(x => x.Id).ToArray();
        var activeActivity = activities.FirstOrDefault(x => string.Equals(x.Id, _state.ActiveActivityId, StringComparison.Ordinal))
            ?? activities.FirstOrDefault();
        if (activeActivity is not null)
        {
            SelectActivity(activeActivity);
        }
        else
        {
            _sidebarHost.Child = ErrorBlock("No active plugins.");
        }

        RenderPluginFailures();
    }

    private Border ActivityButton(ActivityDescriptor activity)
    {
        var selected = string.Equals(_state.ActiveActivityId, activity.Id, StringComparison.Ordinal);
        var indicator = new Border()
            .DockLeft()
            .Width(3)
            .WithTheme((t, b) => b.Background(selected ? t.Palette.Accent : Color.FromRgb(0, 0, 0).WithAlpha(0)));

        var button = new Border()
            .MinWidth(42)
            .MinHeight(42)
            .ToolTip(activity.Title)
            .Child(new DockPanel().Children(
                indicator,
                new TextBlock()
                    .Text(activity.Icon ?? activity.Title[..1])
                    .FontSize(14)
                    .Center()));

        button.MouseDown += e =>
        {
            if (e.Button != MouseButton.Left)
            {
                return;
            }

            SelectActivity(activity);
            e.Handled = true;
        };
        return button;
    }

    private void RenderActivityBar()
    {
        _activityBar.Clear();
        foreach (var activity in _pluginHost.VisibleContributions.Activities.OrderBy(x => x.Order).ThenBy(x => x.Id))
        {
            _activityBar.Children(ActivityButton(activity));
        }
    }

    private void SelectActivity(ActivityDescriptor activity)
    {
        var container = _pluginHost.VisibleContributions.ViewContainers.FirstOrDefault(x => x.Id == activity.ViewContainerId);
        if (container is null)
        {
            _logger.Error("Workbench", $"Missing view container '{activity.ViewContainerId}' for activity '{activity.Id}'.");
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
        _state.SetActiveActivity(activity.Id);
        RenderActivityBar();
    }

    private FrameworkElement HostView(IMewooView view)
    {
        return view.NativeView is FrameworkElement element
            ? element
            : UnsupportedViewBlock(view);
    }

    private void AddMainTab(MainViewDescriptor descriptor)
    {
        _mainTabs.AddTab(
            descriptor.Title,
            HostView(descriptor.CreateView(new WorkbenchViewContext(descriptor.OwnerPluginId, _services, this))),
            closable: true,
            onClose: () => _state.RemoveMainView(descriptor.Id));
    }

    private static TextBlock MenuText(string text) => new TextBlock()
        .Text(text)
        .FontSize(12)
        .CenterVertical()
        .Margin(8, 0);

    private static PathShape ThemeIcon() => new PathShape()
        .Center()
        .Size(14)
        .Stretch(Stretch.Uniform)
        .WithTheme((t, s) => s.Data(t.IsDark ? LightThemeIcon : DarkThemeIcon).Fill(t.Palette.WindowText));

    private static PathShape PathIcon(PathGeometry data, double size) => new PathShape()
        .Center()
        .Size(size)
        .Stretch(Stretch.Uniform)
        .WithTheme((t, s) => s.Data(data).Fill(t.Palette.WindowText));

    private static Button TitleChromeButton(Element icon, string tooltip, Action onClick)
    {
        var button = new Button()
        {
            Content = icon,
            CornerRadius = 0,
            StyleName = "mewoo.chrome",
            MinWidth = 36,
            MinHeight = 34,
        }
            .ToolTip(tooltip)
            .OnClick(onClick);

        return button;
    }

    private void ToggleSidebar()
    {
        _state.ToggleSidebar();
    }

    private void TogglePanel()
    {
        _state.TogglePanel();
    }

    private void ApplyState()
    {
        _sidebarShell.IsVisible = !_state.SidebarCollapsed || _isResizingSidebar;
        _sidebarShell.Width = _state.SidebarCollapsed ? 0 : _state.SidebarWidth;
        _panelHost.IsVisible = _state.PanelVisible;
        _panelHost.Height = _state.PanelHeight;
    }

    private void ShowLogs()
    {
        if (!_state.PanelVisible)
        {
            _state.TogglePanel();
        }

        RenderLogs();
    }

    private void OnWorkbenchStateChanged()
    {
        ApplyState();
        QueueSaveState();
    }

    private void OnThemeChanged()
    {
        RenderThemeStatus();
        QueueSaveState();
    }

    private void ToggleAlwaysOnTop()
    {
        Topmost = !Topmost;
        QueueSaveState();
    }

    private void QueueSaveState()
    {
        if (_stateStorage is null || _isRestoringState)
        {
            return;
        }

        _ = SaveStateAsync();
    }

    private async Task SaveStateAsync()
    {
        await _stateSaveLock.WaitAsync();
        try
        {
            var snapshot = _state.CreateSnapshot(_themeController.CurrentTheme.Id, Topmost);
            await _stateStorage!.WriteJsonAsync("workbench", snapshot);
        }
        catch
        {
            // Slice 012 will surface storage/logging failures in the UI.
        }
        finally
        {
            _stateSaveLock.Release();
        }
    }

    private void RenderThemeStatus()
    {
        var existing = _statusRight.Children
            .OfType<TextBlock>()
            .FirstOrDefault(x => string.Equals(x.Text, "Dark", StringComparison.Ordinal)
                || string.Equals(x.Text, "Light", StringComparison.Ordinal));

        if (existing is not null)
        {
            existing.Text = _themeController.CurrentTheme.DisplayName;
            return;
        }

        _statusRight.Children(new TextBlock()
            .Text(_themeController.CurrentTheme.DisplayName)
            .FontSize(12)
            .CenterVertical()
            .Margin(8, 0));
    }

    private void RenderLogs()
    {
        _logsPanel.Clear();

        var entries = _logger.Entries
            .TakeLast(100)
            .ToArray();

        if (entries.Length == 0)
        {
            _logsPanel.Children(new TextBlock()
                .Text("No logs yet.")
                .Margin(12)
                .FontSize(12));
            return;
        }

        foreach (var entry in entries)
        {
            var message = $"[{entry.Timestamp:HH:mm:ss}] {entry.Level} {entry.Source}: {entry.Message}";
            if (!string.IsNullOrWhiteSpace(entry.Exception))
            {
                message += $" - {entry.Exception.Split(Environment.NewLine)[0]}";
            }

            _logsPanel.Children(new TextBlock()
                .Text(message)
                .FontSize(12)
                .Margin(12, 2));
        }
    }

    private void RenderPluginFailures()
    {
        var failed = _pluginHost.Plugins.FirstOrDefault(entry => entry.State == Mewoo.Abstractions.Plugins.MewooPluginState.Failed);
        if (failed is null)
        {
            return;
        }

        _sidebarHost.Child = ErrorBlock(
            $"{failed.Plugin.DisplayName} unavailable",
            failed.Error?.Message ?? "Plugin failed.");
    }

    private FrameworkElement UnsupportedViewBlock(IMewooView view)
    {
        _logger.Error("Workbench", $"View '{view.Id}' returned unsupported native view '{view.NativeView.GetType().FullName}'.");
        return ErrorBlock("View failed to load", $"View '{view.Id}' returned unsupported native view.");
    }

    private static TextBlock EmptyBlock(string message) => new TextBlock()
        .Text(message)
        .Margin(16);

    private FrameworkElement ErrorBlock(string title, string? detail = null)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical }
            .Spacing(8)
            .Margin(16)
            .Children(
                new TextBlock()
                    .Text(title)
                    .SemiBold()
                    .Foreground(Color.FromRgb(232, 17, 35)));

        if (!string.IsNullOrWhiteSpace(detail))
        {
            panel.Children(new TextBlock().Text(detail).FontSize(12));
        }

        panel.Children(new Button()
            .Content("Open Logs")
            .OnClick(ShowLogs));

        return panel;
    }

    private sealed record WorkbenchViewContext(string PluginId, IServiceProvider Services, IWorkbenchService Workbench) : IMewooViewContext;

    private sealed class NullMewooLogger : IMewooLogger
    {
        public event Action? Changed { add { } remove { } }

        public IReadOnlyList<MewooLogEntry> Entries => [];

        public void Info(string source, string message) { }

        public void Error(string source, string message, Exception? exception = null) { }
    }
}

public sealed record MewooCommandContext(IServiceProvider Services, IWorkbenchService Workbench) : IMewooCommandContext;
