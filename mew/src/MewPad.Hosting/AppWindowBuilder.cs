namespace MewPad.Hosting;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewPad.Core;
using MewPad.Core.Interfaces;
using MewPad.Core.Shell;
using MewPad.Core.Components.ClosableTabControl;
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
        var initialTheme = savedTheme switch
        {
            "Light" => AppTheme.Light,
            "Dark" => AppTheme.Dark,
            "System" => AppTheme.System,
            _ => AppTheme.System,
        };
        // Set the initial theme to both shell and Application
        shell.Theme.Set(initialTheme);
        ThemeManager.Default = ToThemeVariant(shell.Theme.Current);
        if (Application.IsRunning)
            Application.Current.SetTheme(ToThemeVariant(shell.Theme.Current));
        // Keep MewUI default seeds for balanced contrast/spacing behavior.
        ThemeManager.DefaultLightSeed = ThemeSeed.DefaultLight;
        ThemeManager.DefaultDarkSeed = ThemeSeed.DefaultDark;

        var savedLang = cfg.GetConfig<string>("ui.language");
        if (!string.IsNullOrEmpty(savedLang)) shell.Localization.SetLanguage(savedLang);

        // ── State ────────────────────────────────────────────────
        double cachedSideBarWidth = cfg.GetConfig("ui.sidebarWidth", 240.0);
        double cachedPanelHeight  = cfg.GetConfig("ui.panelHeight", 200.0);
        bool startSideBarCollapsed = cfg.GetConfig("ui.sidebarCollapsed", false);
        bool startPanelCollapsed   = cfg.GetConfig("ui.panelCollapsed", false);
        string? lastActiveActivityId = cfg.GetConfig<string>("ui.lastActiveActivityId");
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
        Action? closeSettings = null;
        Action? showNoActivityState = null;
        bool isSettingsOpen = false;
        string? activityBeforeSettings = null;

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
                {
                    btn.WithTheme((t, b) =>
                    {
                        var alpha = t.IsDark ? (byte)0x55 : (byte)0x33;
                        b.Background = t.Palette.Accent.WithAlpha(alpha);
                    });
                }
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

        // ── ContentArea state — declared early so ActivateActivity can reference them ─
        // Rule: switching Activity always switches BOTH SideBar and ContentArea.
        var contentCache = new Dictionary<string, FrameworkElement>();
        var contentTabItems = new Dictionary<string, TabItem>();            // contentId → TabItem
        var activityTabControls = new Dictionary<string, ClosableTabControl>(); // activityId → ClosableTabControl
        Border contentAreaHolder = null!;    // assigned in ContentArea section below
        FrameworkElement welcomeContent = null!; // assigned in ContentArea section below

        // ── ActivityBar ───────────────────────────────────────────
        var allActivities = shell.GetActivities();
        var topActivities = allActivities.Where(a => a.Section == ActivityBarSection.Top).ToList();
        var bottomActivities = allActivities.Where(a => a.Section == ActivityBarSection.Bottom).ToList();

        // Subscribe to active activity changes → update indicators
        shell.ActiveActivityId.Changed.Subscribe(activeId =>
        {
            var accent = Application.IsRunning ? Application.Current.Theme.Palette.Accent : AccentColor;
            foreach (var (id, indicator) in activityIndicators)
                indicator.Background = id == activeId ? accent : TransparentColor;
        });

        shell.Theme.Changed.Subscribe(_ =>
        {
            var activeId = shell.ActiveActivityId.Value;
            var accent = Application.IsRunning ? Application.Current.Theme.Palette.Accent : AccentColor;
            foreach (var (id, indicator) in activityIndicators)
                indicator.Background = id == activeId ? accent : TransparentColor;
        });

        FrameworkElement MakeActivityButton(IActivityItem activity)
        {
            var id = activity.Id;
            var iconText = activity.Icon.ToString() ?? activity.Title[..1];
            var iconLabel = new Label
            {
                Text = iconText,
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            }.WithTheme((t, l) => l.Foreground = ResolveTextColor(shell, t));

            var indicator = new Border { Width = 2, Background = TransparentColor };
            activityIndicators[id] = indicator;

            var btn = new Button
            {
                Content = iconLabel,
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
        var settingsIconLabel = new Label
        {
            Text = "⚙",
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        }.WithTheme((t, l) => l.Foreground = ResolveTextColor(shell, t));

        var settingsBtn = new Button
        {
            Content = settingsIconLabel,
            MinWidth = 48,
            MinHeight = 46,
        }.OnClick(() =>
        {
            // Toggle settings: click once to open, click again to close
            if (isSettingsOpen && closeSettings != null)
            {
                closeSettings();
            }
            else
            {
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

        // ── ContentArea ────────────────────────────────────────────
        contentAreaHolder = new Border();

        welcomeContent = new StackPanel
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
                new Button { Content = new Label { Text = "Switch Theme" }, MinWidth = 120, Margin = new Thickness(0, 16, 0, 0) }.OnClick(() =>
                {
                    shell.Theme.Toggle();
                    Application.Current.SetTheme(ToThemeVariant(shell.Theme.Current));
                })
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

        contentAreaHolder.Child = welcomeContent;
        // Mark to collapse SideBar when showing welcome page (will be applied after mainSplit is initialized)
        bool shouldCollapseSideBar = true;
        bool shouldCollapsePanel = true;

        openSettingsAction = categoryId =>
        {
            if (settingsCategories.Count == 0)
                return;

            if (!isSettingsOpen)
                activityBeforeSettings = shell.ActiveActivityId.Value;
            isSettingsOpen = true;

            var selectedCategory = settingsCategories.FirstOrDefault(c => c.Id == categoryId) ?? settingsCategories[0];
            activeSettingsCategoryId = selectedCategory.Id;
            shell.ActiveActivityId.Value = "mewpad.settings";
            ShowSettingsSideBar();

            if (mainSplit != null && shell.SideBarCollapsed.Value)
                ToggleSideBar();

            var item = new SettingsContentItem(shell, selectedCategory.Id);
            contentAreaHolder.Child = item.CreateContent();
        };

        closeSettings = () =>
        {
            isSettingsOpen = false;

            var restoreActivityId = activityBeforeSettings;
            activityBeforeSettings = null;

            if (!string.IsNullOrEmpty(restoreActivityId))
            {
                shell.ActiveActivityId.Value = restoreActivityId;
                RestorePrimarySideBar();

                // Restore this activity's TabControl (it remembers its selected tab).
                if (activityTabControls.TryGetValue(restoreActivityId, out var tc) && tc.Count > 0)
                    contentAreaHolder.Child = tc.Inner;
                else
                    contentAreaHolder.Child = welcomeContent;

                if (mainSplit != null && shell.SideBarCollapsed.Value)
                    ToggleSideBar();
                return;
            }

            showNoActivityState?.Invoke();
        };

        showNoActivityState = () =>
        {
            isSettingsOpen = false;
            activityBeforeSettings = null;
            shell.ActiveActivityId.Value = null;
            contentAreaHolder.Child = welcomeContent;
            shouldCollapseSideBar = true;
            shouldCollapsePanel = true;

            if (!shell.SideBarCollapsed.Value)
                ToggleSideBar();
            if (!shell.PanelCollapsed.Value)
                TogglePanel();
        };

        // Rule: switching an Activity always switches BOTH SideBar and ContentArea in sync.
        // SideBar gets the activity's navigation panel; ContentArea shows that activity's
        // TabControl (which remembers its last selected tab), or the welcome screen if empty.
        void ActivateActivity(string activityId)
        {
            if (isSettingsOpen && closeSettings != null)
            {
                isSettingsOpen = false;
                closeSettings();
            }

            if (shell.ActiveActivityId.Value == activityId)
                return;

            var activity = shell.GetActivity(activityId);
            if (activity == null) return;

            shell.ActiveActivityId.Value = activityId;
            sideBarTitle.Text = activity.Title.ToUpperInvariant();

            // Switch SideBar.
            if (!activityContentCache.TryGetValue(activityId, out var sideContent))
            {
                sideContent = activity.CreateContent();
                activityContentCache[activityId] = sideContent;
            }
            sideBarPanel!.Second = sideContent;

            // Switch ContentArea: the TabControl remembers its own selected tab.
            if (activityTabControls.TryGetValue(activityId, out var tc) && tc.Count > 0)
                contentAreaHolder.Child = tc.Inner;
            else
                contentAreaHolder.Child = welcomeContent;

            if (shell.SideBarCollapsed.Value)
                ToggleSideBar();
        }

        ClosableTabControl GetOrCreateTabControl(string activityId)
        {
            if (!activityTabControls.TryGetValue(activityId, out var ctc))
            {
                ctc = new ClosableTabControl();
                activityTabControls[activityId] = ctc;
            }
            return ctc;
        }

        shell.ActiveContentId.Changed.Subscribe(id =>
        {
            var activityId = shell.ActiveActivityId.Value;
            if (id == null)
            {
                // Content closed externally: show remaining tabs or welcome.
                if (!string.IsNullOrEmpty(activityId) &&
                    activityTabControls.TryGetValue(activityId, out var rem) && rem.Count > 0)
                    contentAreaHolder.Child = rem.Inner;
                else
                    contentAreaHolder.Child = welcomeContent;

                if (shell.ActiveActivityId.Value == null && !isSettingsOpen)
                {
                    shouldCollapseSideBar = true;
                    shouldCollapsePanel = true;
                    if (!shell.SideBarCollapsed.Value)
                        ToggleSideBar();
                    if (!shell.PanelCollapsed.Value)
                        TogglePanel();
                }
                return;
            }

            var item = shell.GetContent(id);
            if (item == null) return;
            if (string.IsNullOrEmpty(activityId)) return;

            // If tab already exists, just select it.
            if (contentTabItems.TryGetValue(id, out var existingTab))
            {
                var ctc = GetOrCreateTabControl(activityId);
                for (var i = 0; i < ctc.Inner.Tabs.Count; i++)
                {
                    if (ctc.Inner.Tabs[i] == existingTab)
                    {
                        ctc.Select(i);
                        break;
                    }
                }
                contentAreaHolder.Child = ctc.Inner;
                return;
            }

            // Build content (cached).
            if (!contentCache.TryGetValue(id, out var content))
            {
                content = item.CreateContent();
                contentCache[id] = content;
            }

            // Build tab with close button via ClosableTabControl.
            var tabControl = GetOrCreateTabControl(activityId);
            var capturedId = id;
            var newTab = tabControl.AddClosableTab(
                title: item.Title,
                icon: item.Icon?.ToString(),
                content: content,
                closable: item.CanClose,
                onClose: () =>
                {
                    shell.CloseContent(capturedId);
                    contentTabItems.Remove(capturedId);
                    contentCache.Remove(capturedId);
                    if (capturedId == "mewpad.settings")
                        RestorePrimarySideBar();
                    contentAreaHolder.Child = tabControl.Count > 0 ? tabControl.Inner : welcomeContent;
                }
            );
            contentTabItems[id] = newTab;
            tabControl.SelectLast();
            contentAreaHolder.Child = tabControl.Inner;
        });

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
            Second = contentAreaHolder,
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

        if (shouldCollapsePanel)
        {
            panelShell.Height = 0;
            shell.PanelCollapsed.Value = true;
        }

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
        var appIconLabel = new Label
        {
            Text = "🐾",
            FontSize = 14,
            Margin = new Thickness(8, 0, 4, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
        window.TitleBarLeft.Add(appIconLabel);
        var titleMenuBar = BuildMenuBar(shell, ToggleSideBar, TogglePanel);
        window.TitleBarLeft.Add(titleMenuBar);
        var themeToggleLabel = new Label
        {
            Text = GetThemeModeIcon(shell.Theme.Current),
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
        }.OnClick(() =>
        {
            shell.Theme.Toggle();
            Application.Current.SetTheme(ToThemeVariant(shell.Theme.Current));
            themeToggleLabel.Text = GetThemeModeIcon(shell.Theme.Current);
        });
        window.TitleBarRight.Add(themeToggleButton);

        shell.Theme.Changed.Subscribe(t =>
        {
            themeToggleLabel.Text = GetThemeModeIcon(t);
            Application.Current.SetTheme(ToThemeVariant(t));
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
            cfg.SetConfig("ui.lastActiveActivityId", shell.ActiveActivityId.Value ?? string.Empty);
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
        // Try to restore last active Activity, otherwise use first available
        string? activityToActivate = null;
        if (!string.IsNullOrEmpty(lastActiveActivityId))
        {
            var lastActivity = allActivities.FirstOrDefault(a => a.Id == lastActiveActivityId);
            if (lastActivity != null)
                activityToActivate = lastActiveActivityId;
        }
        if (string.IsNullOrEmpty(activityToActivate))
        {
            var firstActivity = topActivities.Count > 0 ? topActivities[0]
                : bottomActivities.Count > 0 ? bottomActivities[0]
                : null;
            if (firstActivity != null)
                activityToActivate = firstActivity.Id;
        }
        if (!string.IsNullOrEmpty(activityToActivate))
            ActivateActivity(activityToActivate);

        // Auto-save configuration periodically (basic throttling: on state changes)
        shell.ActiveActivityId.Changed.Subscribe(_ =>
            cfg.SetConfig("ui.lastActiveActivityId", shell.ActiveActivityId.Value ?? string.Empty));

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
    private static MenuBar BuildMenuBar(ShellContext shell, Action toggleSideBar, Action togglePanel)
    {
        var fileMenu = new Menu()
            .Item("Exit", () => Application.Quit());

        var viewMenu = new Menu()
            .Item("Toggle SideBar", toggleSideBar)
            .Item("Toggle Panel", togglePanel)
            .Separator()
            .Item("Follow System Theme", () =>
            {
                shell.Theme.Set(AppTheme.System);
                Application.Current.SetTheme(ThemeVariant.System);
            })
            .Item("Light Theme", () =>
            {
                shell.Theme.Set(AppTheme.Light);
                Application.Current.SetTheme(ThemeVariant.Light);
            })
            .Item("Dark Theme", () =>
            {
                shell.Theme.Set(AppTheme.Dark);
                Application.Current.SetTheme(ThemeVariant.Dark);
            });

        var helpMenu = new Menu()
            .Item("About MewPad", () => shell.Settings.OpenSettings("about"));

        var bar = new MenuBar()
            .Background(Color.Transparent);  // Transparent so menu text gets auto-colored from Application.Current.Theme

        bar.Add(new MenuItem("(_F)ile").Menu(fileMenu));
        bar.Add(new MenuItem("(_V)iew").Menu(viewMenu));
        bar.Add(new MenuItem("(_H)elp").Menu(helpMenu));

        return bar;
    }

    // ── StatusBar ─────────────────────────────────────────────────
    private static FrameworkElement BuildStatusBar(ShellContext shell)
    {
        var leftItems = shell.GetStatusBarItems(StatusBarSlot.Left);
        var rightItems = shell.GetStatusBarItems(StatusBarSlot.Right);
        var statusLabels = new List<Label>();

        var leftStack = new StackPanel().Horizontal();
        foreach (var item in leftItems)
        {
            var element = item.CreateElement();
            if (element is Label label)
                statusLabels.Add(label);
            leftStack.Children(element);
        }

        // Default left items when nothing registered
        if (leftItems.Count == 0)
        {
            var branchLabel = new Label { Text = "  ⎇ main  ", FontSize = 11, VerticalAlignment = VerticalAlignment.Center };
            var diagnosticsLabel = new Label { Text = "⊗ 0  ⚠ 0  ", FontSize = 11, VerticalAlignment = VerticalAlignment.Center };
            statusLabels.Add(branchLabel);
            statusLabels.Add(diagnosticsLabel);
            leftStack.Children(branchLabel, diagnosticsLabel);
        }

        var rightStack = new StackPanel().Horizontal();
        foreach (var item in rightItems)
        {
            var element = item.CreateElement();
            if (element is Label label)
                statusLabels.Add(label);
            rightStack.Children(element);
        }

        // Default right items when nothing registered
        if (rightItems.Count == 0)
        {
            var encodingLabel = new Label { Text = "  UTF-8  ", FontSize = 11, VerticalAlignment = VerticalAlignment.Center };
            statusLabels.Add(encodingLabel);
            rightStack.Children(encodingLabel);
        }

        var barDock = new DockPanel();
        DockPanel.SetDock(rightStack, Dock.Right);
        barDock.Add(rightStack);
        barDock.Add(leftStack);

        var bar = new Border { Height = 22, Child = barDock };

        bar.WithTheme((t, b) =>
        {
            b.Background = t.Palette.ButtonFace;
            b.BorderBrush = t.Palette.ControlBorder;
            b.BorderThickness = 1;
            var textColor = ResolveTextColor(shell, t);
            foreach (var label in statusLabels)
                label.Foreground = textColor;
        });

        return bar;
    }

    private static ThemeVariant ToThemeVariant(AppTheme theme)
        => theme switch
        {
            AppTheme.Light => ThemeVariant.Light,
            AppTheme.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.System,
        };

    private static string GetThemeModeIcon(AppTheme theme)
        => theme switch
        {
            AppTheme.System => "🖥",
            AppTheme.Light => "☀",
            _ => "🌙",
        };

    private static bool ResolveIsDark(ShellContext _, Aprillz.MewUI.Theme runtimeTheme)
        => runtimeTheme.IsDark;

    private static Color ResolveTextColor(ShellContext _, Aprillz.MewUI.Theme runtimeTheme)
        => runtimeTheme.Palette.WindowText;
}