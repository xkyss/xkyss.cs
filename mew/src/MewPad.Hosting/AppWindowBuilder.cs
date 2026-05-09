namespace MewPad.Hosting;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewPad.Core;
using MewPad.Core.Interfaces;
using MewPad.Core.Shell;
using MewPad.Hosting.Extensions;
using MewPad.Hosting.Infrastructure;
using AppTheme = MewPad.Core.Services.Theme;

/// <summary>
/// Builds the main shell window according to the layout design.
/// </summary>
public static class AppWindowBuilder
{
    // Accent color for ActivityBar active indicator and StatusBar background
    private static readonly Color AccentColor = Color.FromRgb(0x00, 0x7A, 0xCC);
    private static readonly Color TransparentColor = Color.FromRgb(0, 0, 0).WithAlpha(0);

    public static Window CreateMainWindow(ShellContext shell)
    {
        var window = new NativeCustomWindow();
        window.Title = "MewPad";
        window.Resizable(1200, 800, minWidth: 600, minHeight: 400);
        window.StartCenterScreen();
        BuildShell(window, shell);
        return window;
    }

    // ── Shell Construction ────────────────────────────────────────
    private static void BuildShell(NativeCustomWindow window, ShellContext shell)
    {
        // ── Restore persisted state ───────────────────────────────
        var cfg = shell.Configuration;
        var savedTheme = cfg.GetConfig<string>("ui.theme");
        if (savedTheme == "Dark") shell.Theme.Set(AppTheme.Dark);
        else if (savedTheme == "Light") shell.Theme.Set(AppTheme.Light);

        var savedLang = cfg.GetConfig<string>("ui.language");
        if (!string.IsNullOrEmpty(savedLang)) shell.Localization.SetLanguage(savedLang);

        // ── State ────────────────────────────────────────────────
        double cachedSideBarWidth = cfg.GetConfig("ui.sidebarWidth", 240.0);
        double cachedPanelHeight  = cfg.GetConfig("ui.panelHeight", 200.0);
        bool startSideBarCollapsed = cfg.GetConfig("ui.sidebarCollapsed", false);
        bool startPanelCollapsed   = cfg.GetConfig("ui.panelCollapsed", false);

        SplitPanel? mainSplit = null;
        SplitPanel? contentAndPanel = null;
        SplitPanel? sideBarPanel = null;

        Label sideBarTitle = new() { Text = "", FontSize = 11, FontWeight = FontWeight.SemiBold, Margin = new Thickness(8, 0) };
        var activityContentCache = new Dictionary<string, FrameworkElement>();
        var activityIndicators = new Dictionary<string, Border>();

        // ── Local Functions ───────────────────────────────────────
        void ToggleSideBar()
        {
            var collapsed = !shell.SideBarCollapsed.Value;
            shell.SideBarCollapsed.Value = collapsed;
            if (collapsed)
            {
                if (mainSplit!.FirstLength.IsAbsolute && mainSplit.FirstLength.Value > 0)
                    cachedSideBarWidth = mainSplit.FirstLength.Value;
                mainSplit.FirstLength = GridLength.Pixels(0);
                mainSplit.MinFirst = 0;
            }
            else
            {
                mainSplit!.FirstLength = GridLength.Pixels(cachedSideBarWidth);
                mainSplit.MinFirst = 120;
            }
        }

        void TogglePanel()
        {
            var collapsed = !shell.PanelCollapsed.Value;
            shell.PanelCollapsed.Value = collapsed;
            if (collapsed)
            {
                if (contentAndPanel!.SecondLength.IsAbsolute && contentAndPanel.SecondLength.Value > 0)
                    cachedPanelHeight = contentAndPanel.SecondLength.Value;
                contentAndPanel.SecondLength = GridLength.Pixels(0);
                contentAndPanel.MinSecond = 0;
            }
            else
            {
                contentAndPanel!.SecondLength = GridLength.Pixels(cachedPanelHeight);
                contentAndPanel.MinSecond = 80;
            }
        }

        void ActivateActivity(string activityId)
        {
            if (shell.ActiveActivityId.Value == activityId)
            {
                ToggleSideBar();
                return;
            }
            var activity = shell.GetActivity(activityId);
            if (activity == null) return;

            shell.ActiveActivityId.Value = activityId;
            sideBarTitle.Text = activity.Title.ToUpperInvariant();

            if (!activityContentCache.TryGetValue(activityId, out var content))
            {
                content = activity.CreateContent();
                activityContentCache[activityId] = content;
            }
            sideBarPanel!.Second = content;

            if (shell.SideBarCollapsed.Value)
                ToggleSideBar();
        }

        void OpenSettings(string categoryId = "appearance")
        {
            var item = new SettingsContentItem(shell, categoryId);
            shell.OpenContent(item);
        }

        // Wire SettingsService opener so shell.Settings.OpenSettings() works from anywhere
        shell.Settings.SetOpenHandler(id => OpenSettings(id));

        // ── ActivityBar ───────────────────────────────────────────
        var allActivities = shell.GetActivities();
        var topActivities = allActivities.Where(a => a.Section == ActivityBarSection.Top).ToList();
        var bottomActivities = allActivities.Where(a => a.Section == ActivityBarSection.Bottom).ToList();

        // Subscribe to active activity changes → update indicators
        shell.ActiveActivityId.Changed.Subscribe(activeId =>
        {
            foreach (var (id, indicator) in activityIndicators)
                indicator.Background = id == activeId ? AccentColor : TransparentColor;
        });

        FrameworkElement MakeActivityButton(IActivityItem activity)
        {
            var id = activity.Id;
            var iconText = activity.Icon.ToString() ?? activity.Title[..1];

            var indicator = new Border { Width = 2, Background = TransparentColor };
            activityIndicators[id] = indicator;

            var btn = new Button
            {
                Content = new Label
                {
                    Text = iconText,
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                MinWidth = 46,
                MinHeight = 46,
            }.OnClick(() => ActivateActivity(id));

            var row = new DockPanel();
            DockPanel.SetDock(indicator, Dock.Left);
            row.Add(indicator);
            row.Add(btn);
            return row;
        }

        var topStack = new StackPanel().Vertical();
        foreach (var activity in topActivities)
            topStack.Children(MakeActivityButton(activity));

        foreach (var activity in bottomActivities)
            topStack.Children(MakeActivityButton(activity));

        // Bottom fixed items: Account + Settings
        var accountBtn = new Button
        {
            Content = new Label { Text = "👤", FontSize = 16, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center },
            MinWidth = 48,
            MinHeight = 46,
        };
        var settingsBtn = new Button
        {
            Content = new Label { Text = "⚙", FontSize = 16, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center },
            MinWidth = 48,
            MinHeight = 46,
        }.OnClick(() => OpenSettings());

        var bottomFixed = new StackPanel().Vertical();
        bottomFixed.Children(accountBtn, settingsBtn);

        var activityBar = new DockPanel();
        activityBar.Width = 48;
        activityBar.MinWidth = 48;
        activityBar.MaxWidth = 48;
        DockPanel.SetDock(bottomFixed, Dock.Bottom);
        activityBar.Add(bottomFixed);
        activityBar.Add(topStack);

        // ── SideBar ───────────────────────────────────────────────
        sideBarPanel = new SplitPanel
        {
            Orientation = Orientation.Vertical,
            FirstLength = GridLength.Pixels(28),
            SecondLength = GridLength.Stars(1),
            MinFirst = 28,
            MinSecond = 0,
            First = sideBarTitle,
            Second = new Label { Text = "Select an activity", Margin = new Thickness(8) },
        };
        sideBarPanel.MinWidth = 120;

        // ── ContentArea — custom tab bar + content border ─────────
        var openTabItems = new List<IContentItem>();
        var contentCache = new Dictionary<string, FrameworkElement>();
        string? activeTabId = null;

        var tabHeadersBorder = new Border();
        var contentBodyBorder = new Border();

        FrameworkElement welcomeContent = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
        }.Vertical().Children(
            new Label { Text = "Welcome to MewPad", FontSize = 22, FontWeight = FontWeight.Bold },
            new Label { Text = "Universal desktop shell workspace", FontSize = 13, Margin = new Thickness(0, 8, 0, 0) });

        contentBodyBorder.Child = welcomeContent;

        void RebuildTabHeaders()
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal };
            foreach (var t in openTabItems.ToList())
            {
                var tab = t;
                var isActive = tab.Id == activeTabId;

                var titleRow = new StackPanel { Orientation = Orientation.Horizontal };
                if (tab.Icon != null)
                    titleRow.Children(new Label { Text = tab.Icon.ToString()!, FontSize = 12, Margin = new Thickness(0, 0, 4, 0) });
                titleRow.Children(new Label { Text = tab.Title });

                if (tab.CanClose)
                {
                    titleRow.Children(new Label { Text = "  " });
                    var closeBtn = new Button
                    {
                        Content = new Label { Text = "×", FontSize = 10 },
                        MinWidth = 16,
                        MinHeight = 14,
                    };
                    closeBtn.Click += () =>
                    {
                        shell.CloseContent(tab.Id);
                        openTabItems.Remove(tab);
                        contentCache.Remove(tab.Id);
                        if (activeTabId == tab.Id)
                        {
                            activeTabId = openTabItems.LastOrDefault()?.Id;
                            contentBodyBorder.Child = activeTabId != null && contentCache.TryGetValue(activeTabId, out var prev)
                                ? prev : welcomeContent;
                        }
                        RebuildTabHeaders();
                    };
                    titleRow.Children(closeBtn);
                }

                var tabBtn = new Button
                {
                    Content = titleRow,
                    Padding = new Thickness(10, 4),
                };
                // Active tab: slightly lighter/highlighted background
                if (isActive)
                    tabBtn.Background = Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF);

                tabBtn.OnClick(() =>
                {
                    activeTabId = tab.Id;
                    if (contentCache.TryGetValue(tab.Id, out var c))
                        contentBodyBorder.Child = c;
                    RebuildTabHeaders();
                });

                row.Children(tabBtn);
            }
            tabHeadersBorder.Child = row;
        }

        RebuildTabHeaders();

        shell.ActiveContentId.Changed.Subscribe(id =>
        {
            if (id == null)
            {
                activeTabId = null;
                contentBodyBorder.Child = welcomeContent;
                RebuildTabHeaders();
                return;
            }

            var item = shell.GetContent(id);
            if (item == null) return;

            if (!openTabItems.Any(t => t.Id == id))
                openTabItems.Add(item);

            if (!contentCache.TryGetValue(id, out var content))
            {
                content = item.CreateContent();
                contentCache[id] = content;
            }

            activeTabId = id;
            contentBodyBorder.Child = content;
            RebuildTabHeaders();
        });

        var contentTabs = new DockPanel();
        DockPanel.SetDock(tabHeadersBorder, Dock.Top);
        contentTabs.Add(tabHeadersBorder);
        contentTabs.Add(contentBodyBorder);

        // ── PanelArea with tool buttons overlay ───────────────────
        var panelTabs = new TabControl();
        foreach (var panel in shell.GetPanels())
        {
            var p = panel;
            panelTabs.AddTabs(new TabItem
            {
                Header = new Label { Text = p.Title },
                Content = p.CreateContent(),
            });
        }
        panelTabs.AddTabs(
            new TabItem { Header = new Label { Text = "Terminal" }, Content = new Label { Text = "Terminal panel" } },
            new TabItem { Header = new Label { Text = "Output" }, Content = new Label { Text = "Output panel" } }
        );

        // Overlay collapse button at top-right of the panel area
        var panelCollapseBtn = new Button
        {
            Content = new Label { Text = "⊟", FontSize = 11 },
            MinWidth = 26,
            MinHeight = 22,
        }.OnClick(TogglePanel);

        var panelTools = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 2, 4, 0),
        };
        panelTools.Children(panelCollapseBtn);

        var panelContainer = new Grid();
        panelContainer.Rows("*");
        Grid.SetRow(panelTabs, 0);
        panelContainer.Add(panelTabs);
        Grid.SetRow(panelTools, 0);
        panelContainer.Add(panelTools);

        // ── Layout ────────────────────────────────────────────────
        contentAndPanel = new SplitPanel
        {
            Orientation = Orientation.Vertical,
            FirstLength = GridLength.Stars(1),
            SecondLength = GridLength.Pixels(cachedPanelHeight),
            MinFirst = 200,
            MinSecond = 80,
            First = contentTabs,
            Second = panelContainer,
        };

        mainSplit = new SplitPanel
        {
            Orientation = Orientation.Horizontal,
            FirstLength = GridLength.Pixels(cachedSideBarWidth),
            SecondLength = GridLength.Stars(1),
            MinFirst = 120,
            MinSecond = 200,
            First = sideBarPanel,
            Second = contentAndPanel,
        };

        var mainArea = new DockPanel();
        DockPanel.SetDock(activityBar, Dock.Left);
        mainArea.Add(activityBar);
        mainArea.Add(mainSplit);

        // ── StatusBar ─────────────────────────────────────────────
        var statusBar = BuildStatusBar(shell);

        // ── Root ──────────────────────────────────────────────────
        var root = new Grid();
        root.Rows("*,auto");
        Grid.SetRow(mainArea, 0);
        Grid.SetRow(statusBar, 1);
        root.Add(mainArea);
        root.Add(statusBar);

        // ── TitleBar ──────────────────────────────────────────────
        window.TitleBarLeft.Add(new Label
        {
            Text = "🐾",
            FontSize = 14,
            Margin = new Thickness(8, 0, 4, 0),
            VerticalAlignment = VerticalAlignment.Center,
        });
        window.TitleBarLeft.Add(BuildMenuBar(shell, ToggleSideBar, TogglePanel));
        window.TitleBarRight.Add(new Button
        {
            Content = new Label { Text = "🌙", FontSize = 13 },
            MinWidth = 32,
            MinHeight = 28,
        }.OnClick(() => shell.Theme.Toggle()));

        window.Content = root;
        window.Padding = new Thickness(0);

        // ── Persist config on window close ────────────────────────
        window.Closed += () =>
        {
            cfg.SetConfig("ui.theme", shell.Theme.Current.ToString());
            cfg.SetConfig("ui.language", shell.Localization.CurrentLanguage);
            cfg.SetConfig("ui.sidebarCollapsed", shell.SideBarCollapsed.Value);
            cfg.SetConfig("ui.panelCollapsed", shell.PanelCollapsed.Value);
            if (mainSplit!.FirstLength.IsAbsolute && mainSplit.FirstLength.Value > 0)
                cfg.SetConfig("ui.sidebarWidth", mainSplit.FirstLength.Value);
            if (contentAndPanel!.SecondLength.IsAbsolute && contentAndPanel.SecondLength.Value > 0)
                cfg.SetConfig("ui.panelHeight", contentAndPanel.SecondLength.Value);
            cfg.Save();
        };

        // ── Default activation + restore sidebar/panel state ─────
        var firstActivity = topActivities.Count > 0 ? topActivities[0]
            : bottomActivities.Count > 0 ? bottomActivities[0]
            : null;
        if (firstActivity != null)
            ActivateActivity(firstActivity.Id);

        // Apply persisted collapse states after initial activation
        if (startSideBarCollapsed)
        {
            shell.SideBarCollapsed.Value = false; // ensure toggle flips correctly
            ToggleSideBar();
        }
        if (startPanelCollapsed)
        {
            shell.PanelCollapsed.Value = false;
            TogglePanel();
        }
    }

    // ── MenuBar ───────────────────────────────────────────────────
    private static FrameworkElement BuildMenuBar(ShellContext shell, Action toggleSideBar, Action togglePanel)
    {
        var fileMenu = new Menu()
            .Item("Exit", () => Application.Quit());

        var viewMenu = new Menu()
            .Item("Toggle SideBar", toggleSideBar)
            .Item("Toggle Panel", togglePanel)
            .Separator()
            .Item("Light Theme", () => shell.Theme.Set(AppTheme.Light))
            .Item("Dark Theme", () => shell.Theme.Set(AppTheme.Dark));

        var helpMenu = new Menu()
            .Item("About MewPad", () => { });

        var bar = new MenuBar();
        bar.Background = TransparentColor;
        bar.Add(new MenuItem("File(_F)").Menu(fileMenu));
        bar.Add(new MenuItem("View(_V)").Menu(viewMenu));
        bar.Add(new MenuItem("Help(_H)").Menu(helpMenu));
        return bar;
    }

    // ── StatusBar ─────────────────────────────────────────────────
    private static FrameworkElement BuildStatusBar(ShellContext shell)
    {
        var leftItems = shell.GetStatusBarItems(StatusBarSlot.Left);
        var rightItems = shell.GetStatusBarItems(StatusBarSlot.Right);

        var leftStack = new StackPanel().Horizontal();
        foreach (var item in leftItems)
            leftStack.Children(item.CreateElement());

        // Default left items when nothing registered
        if (leftItems.Count == 0)
            leftStack.Children(
                new Label { Text = "  ⎇ main  ", FontSize = 11, VerticalAlignment = VerticalAlignment.Center },
                new Label { Text = "⊗ 0  ⚠ 0  ", FontSize = 11, VerticalAlignment = VerticalAlignment.Center });

        var rightStack = new StackPanel().Horizontal();
        foreach (var item in rightItems)
            rightStack.Children(item.CreateElement());

        // Default right items when nothing registered
        if (rightItems.Count == 0)
            rightStack.Children(
                new Label { Text = "  UTF-8  ", FontSize = 11, VerticalAlignment = VerticalAlignment.Center });

        var barDock = new DockPanel();
        DockPanel.SetDock(rightStack, Dock.Right);
        barDock.Add(rightStack);
        barDock.Add(leftStack);

        var bar = new Border { Height = 22, Background = AccentColor, Child = barDock };

        // Update background when theme changes
        shell.Theme.Changed.Subscribe(_ => bar.Background = AccentColor);

        return bar;
    }
}