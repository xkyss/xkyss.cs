namespace MewPad.Hosting;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewPad.Core.Interfaces;
using MewPad.Core.Shell;

/// <summary>
/// Builds the main shell window according to the layout design.
/// </summary>
public static class AppWindowBuilder
{
    public static Window CreateMainWindow(ShellContext shell)
    {
        var root = new Grid();
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Stars(1) });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

        var layout = BuildMainLayout(shell);

        Grid.SetRow(layout.TitleBar, 0);
        Grid.SetRow(layout.MainArea, 1);
        Grid.SetRow(layout.StatusBar, 2);

        root.Add(layout.TitleBar);
        root.Add(layout.MainArea);
        root.Add(layout.StatusBar);

        return new Window()
            .Title("MewPad - Universal Desktop Framework")
            .Resizable(1280, 800)
            .Content(root);
    }

    private static (FrameworkElement TitleBar, FrameworkElement MainArea, FrameworkElement StatusBar) BuildMainLayout(ShellContext shell)
    {
        var sideBarTitle = new Label { Text = "" };
        var panelState = new Label { Text = "Panel: expanded" };
        var statusInfo = new Label { Text = "Ready" };

        double cachedSideBarWidth = 240;
        double cachedPanelHeight = 200;

        SplitPanel? mainSplit = null;
        SplitPanel? contentAndPanel = null;

        // Cache activity content to avoid recreating on every switch
        var activityContentCache = new Dictionary<string, FrameworkElement>();

        var panelTabs = CreatePanelTabs(shell);
        var contentTabs = CreateContentTabs();

        // SideBar: SplitPanel(Vertical) so we can swap Second for dynamic activity content
        var sideBar = new SplitPanel
        {
            Orientation = Orientation.Vertical,
            FirstLength = GridLength.Pixels(28),
            SecondLength = GridLength.Stars(1),
            MinFirst = 28,
            MinSecond = 0,
            First = sideBarTitle,
            Second = new Label { Text = "Select an activity" }
        };
        sideBar.MinWidth = 120;

        contentAndPanel = new SplitPanel
        {
            Orientation = Orientation.Vertical,
            FirstLength = GridLength.Stars(1),
            SecondLength = GridLength.Pixels(cachedPanelHeight),
            MinFirst = 200,
            MinSecond = 80,
            First = contentTabs,
            Second = panelTabs,
        };

        mainSplit = new SplitPanel
        {
            Orientation = Orientation.Horizontal,
            FirstLength = GridLength.Pixels(cachedSideBarWidth),
            SecondLength = GridLength.Stars(1),
            MinFirst = 120,
            MinSecond = 200,
            First = sideBar,
            Second = contentAndPanel,
        };

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
                statusInfo.Text = "SideBar collapsed";
                return;
            }

            mainSplit!.FirstLength = GridLength.Pixels(cachedSideBarWidth);
            mainSplit.MinFirst = 120;
            statusInfo.Text = "SideBar expanded";
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
                panelState.Text = "Panel: collapsed";
                statusInfo.Text = "Panel collapsed";
                return;
            }

            contentAndPanel!.SecondLength = GridLength.Pixels(cachedPanelHeight);
            contentAndPanel.MinSecond = 80;
            panelState.Text = "Panel: expanded";
            statusInfo.Text = "Panel expanded";
        }

        void ActivateActivity(string activityId)
        {
            // Clicking the already-active activity toggles SideBar visibility
            if (shell.ActiveActivityId.Value == activityId)
            {
                ToggleSideBar();
                return;
            }

            var activity = shell.GetActivity(activityId);
            if (activity == null)
            {
                statusInfo.Text = $"Activity '{activityId}' not found";
                return;
            }

            shell.ActiveActivityId.Value = activityId;
            sideBarTitle.Text = activity.Title;

            // Load activity content (cached to preserve state across switches)
            if (!activityContentCache.TryGetValue(activityId, out var content))
            {
                content = activity.CreateContent();
                activityContentCache[activityId] = content;
            }

            // Swap SideBar content
            sideBar.Second = content;

            // Expand SideBar if it was collapsed
            if (shell.SideBarCollapsed.Value)
                ToggleSideBar();

            statusInfo.Text = "Switched to " + activity.Title;
        }

        // Build ActivityBar dynamically from all registered activities
        var allActivities = shell.GetActivities();
        var topActivities = allActivities.Where(a => a.Section == ActivityBarSection.Top).ToList();
        var bottomActivities = allActivities.Where(a => a.Section == ActivityBarSection.Bottom).ToList();

        var activityBar = new StackPanel().Vertical();
        activityBar.Width = 48;
        activityBar.MinWidth = 48;
        activityBar.MaxWidth = 48;

        foreach (var activity in topActivities)
        {
            var id = activity.Id;
            var iconText = activity.Icon?.ToString() ?? activity.Title[..1];
            activityBar.Children(new Button().Content(iconText).OnClick(() => ActivateActivity(id)));
        }

        if (topActivities.Count > 0 && bottomActivities.Count > 0)
            activityBar.Children(new Label { Text = "-" });

        foreach (var activity in bottomActivities)
        {
            var id = activity.Id;
            var iconText = activity.Icon?.ToString() ?? activity.Title[..1];
            activityBar.Children(new Button().Content(iconText).OnClick(() => ActivateActivity(id)));
        }

        // Activate the first activity by default
        if (topActivities.Count > 0)
            ActivateActivity(topActivities[0].Id);
        else if (bottomActivities.Count > 0)
            ActivateActivity(bottomActivities[0].Id);

        var mainArea = new DockPanel();
        DockPanel.SetDock(activityBar, Dock.Left);
        mainArea.Add(activityBar);
        mainArea.Add(mainSplit);

        var titleBar = new StackPanel()
            .Horizontal()
            .Children(
                new Label { Text = "[App] MewPad" },
                new Label { Text = "  File  Edit  View  Help" },
                new Button().Content("Toggle Theme").OnClick(() =>
                {
                    shell.Theme.Toggle();
                    statusInfo.Text = $"Theme: {shell.Theme.Current}";
                }),
                new Button().Content("Toggle Panel").OnClick(TogglePanel)
            );

        var statusBar = new StackPanel()
            .Horizontal()
            .Children(
                new Label { Text = "Git: main" },
                new Label { Text = "Errors: 0 Warnings: 0" },
                panelState,
                statusInfo,
                new Label { Text = "UTF-8" }
            );

        return (titleBar, mainArea, statusBar);
    }

    private static TabControl CreateContentTabs()
    {
        var tabs = new TabControl();
        tabs.AddTabs(
            new TabItem
            {
                Header = new Label { Text = "Welcome" },
                Content = new StackPanel().Vertical().Children(
                    new Label { Text = "Welcome to MewPad" },
                    new Label { Text = "Universal desktop shell workspace" })
            }
        );
        return tabs;
    }

    private static TabControl CreatePanelTabs(ShellContext shell)
    {
        var tabs = new TabControl();

        // Add registered panel items
        foreach (var panel in shell.GetPanels())
        {
            var p = panel;
            tabs.AddTabs(new TabItem
            {
                Header = new Label { Text = p.Title },
                Content = p.CreateContent()
            });
        }

        // Always include Terminal and Output as built-in panels
        tabs.AddTabs(
            new TabItem
            {
                Header = new Label { Text = "Terminal" },
                Content = new Label { Text = "Terminal panel" }
            },
            new TabItem
            {
                Header = new Label { Text = "Output" },
                Content = new Label { Text = "Output panel" }
            }
        );

        return tabs;
    }
}