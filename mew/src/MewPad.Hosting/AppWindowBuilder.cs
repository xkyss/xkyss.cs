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
        BuildShell(window, shell);
        return window;
    }

    // ── Shell Construction ────────────────────────────────────────
    private static void BuildShell(NativeCustomWindow window, ShellContext shell)
    {
        // ── Restore persisted state ───────────────────────────────
        var cfg = shell.Configuration;
        var savedTheme = cfg.GetConfig<string>("ui.theme");
        if (savedTheme == "Light") shell.Theme.Set(AppTheme.Light);
        else shell.Theme.Set(AppTheme.Dark);  // Default to Dark theme

        ThemeManager.Default = shell.Theme.Current == AppTheme.Dark ? ThemeVariant.Dark : ThemeVariant.Light;

        var savedLang = cfg.GetConfig<string>("ui.language");
        if (!string.IsNullOrEmpty(savedLang)) shell.Localization.SetLanguage(savedLang);

        // ── State ────────────────────────────────────────────────
        double cachedSideBarWidth = cfg.GetConfig("ui.sidebarWidth", 240.0);
        double cachedPanelHeight  = cfg.GetConfig("ui.panelHeight", 200.0);
        bool startSideBarCollapsed = cfg.GetConfig("ui.sidebarCollapsed", false);
        bool startPanelCollapsed   = cfg.GetConfig("ui.panelCollapsed", false);
        double savedWindowWidth = cfg.GetConfig("ui.windowWidth", 1200.0);
        double savedWindowHeight = cfg.GetConfig("ui.windowHeight", 800.0);
        double savedWindowX = cfg.GetConfig("ui.windowX", double.NaN);
        double savedWindowY = cfg.GetConfig("ui.windowY", double.NaN);
        bool savedWindowMaximized = cfg.GetConfig("ui.windowMaximized", false);

        window.Resizable(savedWindowWidth, savedWindowHeight, minWidth: 600, minHeight: 400);
        if (!double.IsNaN(savedWindowX) && !double.IsNaN(savedWindowY) && !savedWindowMaximized)
            window.StartManualPosition(savedWindowX, savedWindowY);
        else if (!savedWindowMaximized)
            window.StartCenterScreen();
        if (savedWindowMaximized)
            window.WindowState = WindowState.Maximized;

        SplitPanel? mainSplit = null;
        Border? panelShell = null;
        SplitPanel? sideBarPanel = null;

        Label sideBarTitle = new() { Text = "", FontSize = 11, FontWeight = FontWeight.SemiBold, Margin = new Thickness(8, 0) };
        var activityContentCache = new Dictionary<string, FrameworkElement>();
        var activityIndicators = new Dictionary<string, Border>();
        var settingsCategories = shell.Settings.Categories.OrderBy(c => c.Order).ToList();
        string activeSettingsCategoryId = settingsCategories.FirstOrDefault()?.Id ?? "appearance";
        Action<string> openSettingsAction = _ => { };
        bool isSettingsOpen = false;  // Track settings visibility

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
                if (panelShell != null && panelShell.Height > 0)
                    cachedPanelHeight = panelShell.Height;
                if (panelShell != null)
                    panelShell.Height = 0;
            }
            else
            {
                if (panelShell != null)
                    panelShell.Height = cachedPanelHeight;
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

        FrameworkElement BuildSettingsSideBar()
        {
            var navStack = new StackPanel().Vertical();
            foreach (var category in settingsCategories)
            {
                var c = category;
                var isActive = c.Id == activeSettingsCategoryId;
                var btn = new Button
                {
                    Content = new Label
                    {
                        Text = c.Icon != null ? $"{c.Icon}  {c.Title}" : c.Title,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    },
                    Padding = new Thickness(8, 6),
                    Margin = new Thickness(0, 2),
                };
                if (isActive)
                    btn.Background = Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF);
                btn.OnClick(() => OpenSettings(c.Id));
                navStack.Children(btn);
            }

            return new Border
            {
                Padding = new Thickness(4),
                Child = navStack,
            };
        }

        void RestorePrimarySideBar()
        {
            var activityId = shell.ActiveActivityId.Value;
            if (!string.IsNullOrEmpty(activityId) && activityContentCache.TryGetValue(activityId, out var content))
            {
                var activity = shell.GetActivity(activityId);
                sideBarTitle.Text = activity?.Title.ToUpperInvariant() ?? string.Empty;
                sideBarPanel!.Second = content;
                return;
            }

            sideBarTitle.Text = string.Empty;
            sideBarPanel!.Second = new Label { Text = "Select an activity", Margin = new Thickness(8) };
        }

        void ShowSettingsSideBar()
        {
            sideBarTitle.Text = "SETTINGS";
            sideBarPanel!.Second = BuildSettingsSideBar();
        }

        void OpenSettings(string categoryId = "appearance")
        {
            openSettingsAction(categoryId);
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

        // Bottom fixed items: Settings
        Action? closeSettings = null;
        var settingsBtn = new Button
        {
            Content = new Label { Text = "⚙", FontSize = 16, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center },
            MinWidth = 48,
            MinHeight = 46,
        }.OnClick(() =>
        {
            // Toggle settings: click once to open, click again to close
            if (isSettingsOpen && closeSettings != null)
            {
                isSettingsOpen = false;
                closeSettings();
            }
            else
            {
                isSettingsOpen = true;
                OpenSettings();
            }
        });

        var bottomFixed = new StackPanel().Vertical();
        bottomFixed.Children(settingsBtn);

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
            Margin = new Thickness(24),
        }.Vertical().Children(
            new Label { Text = "Welcome to MewPad", FontSize = 24, FontWeight = FontWeight.Bold, HorizontalAlignment = HorizontalAlignment.Center },
            new Label { Text = "Universal desktop shell workspace", FontSize = 13, Margin = new Thickness(0, 8, 0, 0), HorizontalAlignment = HorizontalAlignment.Center },
            new Label { Text = "No page is open right now.", FontSize = 11, Margin = new Thickness(0, 8, 0, 0), HorizontalAlignment = HorizontalAlignment.Center },
            new StackPanel().Horizontal().Children(
                new Button { Content = new Label { Text = "Open Settings" }, MinWidth = 120, Margin = new Thickness(0, 16, 8, 0) }.OnClick(() => shell.Settings.OpenSettings()),
                new Button { Content = new Label { Text = "Switch Theme" }, MinWidth = 120, Margin = new Thickness(0, 16, 0, 0) }.OnClick(() => shell.Theme.Toggle())
            ),
            new Border
            {
                Margin = new Thickness(0, 16, 0, 0),
                Padding = new Thickness(14, 10),
                Child = new StackPanel().Vertical().Children(
                    new Label { Text = "Quick Start", FontSize = 12, FontWeight = FontWeight.SemiBold, Margin = new Thickness(0, 0, 0, 6) },
                    new Label { Text = "- Open Settings to configure language and appearance", FontSize = 11 },
                    new Label { Text = "- Use the ActivityBar to switch shell areas", FontSize = 11 },
                    new Label { Text = "- Open content from future extensions here", FontSize = 11 }
                )
            }
        );

        contentBodyBorder.Child = welcomeContent;
        // Mark to collapse SideBar when showing welcome page (will be applied after mainSplit is initialized)
        bool shouldCollapseSideBar = true;

        openSettingsAction = categoryId =>
        {
            if (settingsCategories.Count == 0)
                return;

            var selectedCategory = settingsCategories.FirstOrDefault(c => c.Id == categoryId) ?? settingsCategories[0];
            activeSettingsCategoryId = selectedCategory.Id;
            ShowSettingsSideBar();

            var item = new SettingsContentItem(shell, selectedCategory.Id);
            var content = item.CreateContent();
            activeTabId = null;  // Settings is not part of the tab system
            contentBodyBorder.Child = content;
        };

        closeSettings = () =>
        {
            RestorePrimarySideBar();
            contentBodyBorder.Child = welcomeContent;
            activeTabId = null;
            // Collapse SideBar when showing welcome page
            shouldCollapseSideBar = true;
            if (!shell.SideBarCollapsed.Value)
                ToggleSideBar();
        };

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
                        if (tab.Id == "mewpad.settings")
                            RestorePrimarySideBar();
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
                // Collapse SideBar when showing welcome page
                shouldCollapseSideBar = true;
                if (!shell.SideBarCollapsed.Value)
                    ToggleSideBar();
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
        mainSplit = new SplitPanel
        {
            Orientation = Orientation.Horizontal,
            FirstLength = GridLength.Pixels(cachedSideBarWidth),
            SecondLength = GridLength.Stars(1),
            MinFirst = 120,
            MinSecond = 200,
            First = sideBarPanel,
            Second = contentTabs,
        };

        // Apply initial state: collapse SideBar for welcome page
        if (shouldCollapseSideBar && mainSplit!.FirstLength.IsAbsolute && mainSplit.FirstLength.Value > 0)
        {
            cachedSideBarWidth = mainSplit.FirstLength.Value;
            mainSplit.FirstLength = GridLength.Pixels(0);
            mainSplit.MinFirst = 0;
            shell.SideBarCollapsed.Value = true;
        }

        panelShell = new Border
        {
            Height = cachedPanelHeight,
            Child = panelContainer,
        };

        // ── StatusBar ─────────────────────────────────────────────
        var statusBar = BuildStatusBar(shell);

        // ── Root ──────────────────────────────────────────────────
        var root = new Grid();
        root.Rows("*,auto,auto");
        root.Columns("48,*");

        Grid.SetRow(activityBar, 0);
        Grid.SetColumn(activityBar, 0);
        Grid.SetRowSpan(activityBar, 2);
        root.Add(activityBar);

        Grid.SetRow(mainSplit, 0);
        Grid.SetColumn(mainSplit, 1);
        root.Add(mainSplit);

        Grid.SetRow(panelShell, 1);
        Grid.SetColumn(panelShell, 1);
        root.Add(panelShell);

        Grid.SetRow(statusBar, 2);
        Grid.SetColumn(statusBar, 0);
        Grid.SetColumnSpan(statusBar, 2);
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
        var themeToggleLabel = new Label
        {
            Text = shell.Theme.Current == AppTheme.Dark ? "☀" : "◐",
            FontSize = 11,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var themeToggleButton = new Button
        {
            Content = themeToggleLabel,
            MinWidth = 46,
            MinHeight = 32,
            StyleName = "chrome",
        }.OnClick(() => shell.Theme.Toggle());
        window.TitleBarRight.Add(themeToggleButton);

        shell.Theme.Changed.Subscribe(t =>
        {
            themeToggleLabel.Text = t == AppTheme.Dark ? "☀" : "◐";

            if (Application.IsRunning)
                Application.Current.SetTheme(t == AppTheme.Dark ? ThemeVariant.Dark : ThemeVariant.Light);
        });

        // Global keyboard shortcuts
        window.KeyBindings.Add(new KeyBinding(new KeyGesture(Key.B, ModifierKeys.Primary), ToggleSideBar));
        window.OnPreviewKeyDown(e =>
        {
            if (!e.PrimaryKey)
                return;

            if (e.PlatformKey == 0xBC)
            {
                shell.Settings.OpenSettings();
                e.Handled = true;
                return;
            }

            if (e.PlatformKey == 0xC0)
            {
                TogglePanel();
                e.Handled = true;
            }
        });

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
            if (panelShell != null && panelShell.Height > 0)
                cfg.SetConfig("ui.panelHeight", panelShell.Height);
            var restoreBounds = window.WindowState == WindowState.Maximized ? window.RestoreBounds : default;
            var windowBounds = window.WindowState == WindowState.Maximized && restoreBounds.Width > 0 && restoreBounds.Height > 0
                ? restoreBounds
                : new Rect(window.Position.X, window.Position.Y, window.ClientSize.Width, window.ClientSize.Height);
            cfg.SetConfig("ui.windowX", windowBounds.X);
            cfg.SetConfig("ui.windowY", windowBounds.Y);
            cfg.SetConfig("ui.windowWidth", windowBounds.Width);
            cfg.SetConfig("ui.windowHeight", windowBounds.Height);
            cfg.SetConfig("ui.windowMaximized", window.WindowState == WindowState.Maximized);
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
            .Item("About MewPad", () => shell.Settings.OpenSettings("about"));

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