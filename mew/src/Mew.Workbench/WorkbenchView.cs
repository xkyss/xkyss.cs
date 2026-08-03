using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.MewDock;

namespace Mew.Workbench;

internal sealed class WorkbenchView
{
    private readonly Workbench _workbench;

    internal WorkbenchView(Workbench workbench) => _workbench = workbench;

    internal UIElement Build()
    {
        var docking = new DockingManager();
        var theme = _workbench.ThemeContext;

        foreach (var view in _workbench.SideBarModel.Views)
        {
            docking.AddToolPane(view.Title, ThemedPane(view.Content, theme, WorkbenchZone.SideBar), DockEdge.Left, view.Id);
        }

        foreach (var document in _workbench.EditorAreaModel.Documents)
        {
            docking.AddDocumentPane(document.Title, ThemedPane(document.Content, theme, WorkbenchZone.EditorArea), document.Id);
        }

        foreach (var view in _workbench.PanelModel.Views)
        {
            docking.AddToolPane(view.Title, ThemedPane(view.Content, theme, WorkbenchZone.Panel), DockEdge.Bottom, view.Id);
        }

        return new Grid()
            .Rows("*,Auto")
            .Columns("Auto,*")
            .Children(
                BuildActivityBar().Row(0).Column(0),
                docking.Row(0).Column(1),
                BuildStatusBar().Row(1).Column(0).ColumnSpan(2)
            );
    }

    private static UIElement ThemedPane(UIElement content, WorkbenchThemeContext theme, WorkbenchZone zone)
    {
        return new Border()
            .Child(content)
            .StretchHorizontal()
            .StretchVertical()
            .WithTheme((_, border) => border.Background(theme.Get(zone).Background));
    }

    private UIElement BuildActivityBar()
    {
        var theme = _workbench.ThemeContext;
        var items = _workbench.ActivityBarModel.Items;
        var children = new Element[items.Count];

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            children[i] = new Button()
                .Size(36, 36)
                .Content(new GlyphElement()
                    .Kind(item.Glyph)
                    .GlyphSize(18)
                    .WithTheme((_, glyph) => glyph.Foreground(theme.ActivityBar.Foreground)))
                .ToolTip(item.Title);
        }

        return new Border()
            .WithTheme((_, border) => border.Background(theme.ActivityBar.Background))
            .Child(
                new StackPanel()
                    .Width(48)
                    .Padding(6, 8)
                    .Spacing(4)
                    .Children(children)
            );
    }

    private UIElement BuildStatusBar()
    {
        var theme = _workbench.ThemeContext;
        var items = _workbench.StatusBarModel.Items;
        var children = new Element[items.Count + 1];

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            children[i] = new Label()
                .Text(item.Text)
                .FontSize(12)
                .WithTheme((_, label) => label.Foreground(theme.StatusBar.Foreground));
        }

        children[items.Count] = new Button()
            .FontSize(12)
            .Padding(8, 4)
            .OnClick(() => CycleTheme(theme))
            .WithTheme((_, button) =>
            {
                button.Content(ThemeModeLabel(theme.Mode));
                button.Foreground(theme.StatusBar.Foreground);
                button.Background(theme.StatusBar.Background);
            });

        return new Border()
            .WithTheme((_, border) => border.Background(theme.StatusBar.Background))
            .Child(
                new StackPanel()
                    .Padding(10, 6)
                    .Spacing(16)
                    .Children(children)
            );
    }

    private static void CycleTheme(WorkbenchThemeContext theme)
    {
        var next = theme.Mode switch
        {
            ThemeVariant.System => ThemeVariant.Light,
            ThemeVariant.Light => ThemeVariant.Dark,
            _ => ThemeVariant.System,
        };

        theme.SetMode(next);
    }

    private static string ThemeModeLabel(ThemeVariant mode) => mode switch
    {
        ThemeVariant.Light => "主题: 亮色",
        ThemeVariant.Dark => "主题: 暗色",
        _ => "主题: 系统",
    };
}
