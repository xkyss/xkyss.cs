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
            IconButton(GlyphKind.Plus, "Toggle Dark/Light theme", _themeController.Toggle),
            IconButton(GlyphKind.ChevronLeft, "Toggle sidebar", ToggleSidebar),
            IconButton(GlyphKind.ChevronUp, "Toggle panel", TogglePanel),
            IconButton(GlyphKind.WindowMaximize, "Always on top", ToggleAlwaysOnTop));
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

        _sidebarShell.DockLeft()
            .Width(_state.SidebarWidth)
            .WithTheme((t, b) => b.Background(t.Palette.ControlBackground))
            .Child(new DockPanel().Children(
                ResizeGrip.Create(ResizeGripOrientation.Vertical, delta =>
                {
                    _state.SetSidebarWidth(_state.SidebarWidth + delta);
                }).DockRight(),
                _sidebarHost));

        var mainColumn = new DockPanel().Children(
            _panelHost.DockBottom(),
            mainArea);

        var body = new DockPanel().Children(
            new Border()
                .DockLeft()
                .MinWidth(42)
                .Width(42)
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

    private static Button IconButton(GlyphKind kind, string tooltip, Action? onClick = null)
    {
        var button = new Button()
            .ToolTip(tooltip)
            .MinWidth(34)
            .MinHeight(34)
            .Content(new GlyphElement().Kind(kind).GlyphSize(5));

        if (onClick is not null)
        {
            button.Click += onClick;
        }

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
        _sidebarShell.IsVisible = !_state.SidebarCollapsed;
        _sidebarShell.Width = _state.SidebarWidth;
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
