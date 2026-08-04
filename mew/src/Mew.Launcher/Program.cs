using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Mew.Launcher;
using Mew.Workbench;

Win32Platform.Register();
Direct2DBackend.Register();

var window = new Window()
    .Title("Mew Launcher — v0.1.1")
    .Resizable(1080, 720);

var store = new LauncherStore();
var items = store.Load();
var categories = items.Select(item => item.Category).Distinct().ToList();

var workbench = new Workbench();
var theme = workbench.ThemeContext;

var listPanel = new StackPanel().Padding(12).Spacing(6);

void ShowCategory(string category)
{
    listPanel.Clear();

    var shown = category == "全部"
        ? items
        : items.Where(item => item.Category == category).ToList();

    listPanel.Add(new Label()
        .Text(category)
        .FontSize(14)
        .Bold()
        .WithTheme((_, label) => label.Foreground(theme.SideBar.Foreground)));

    if (shown.Count == 0)
    {
        listPanel.Add(new Label()
            .Text("暂无启动项")
            .FontSize(12)
            .WithTheme((_, label) => label.Foreground(theme.SideBar.Foreground)));
        return;
    }

    foreach (var item in shown)
    {
        listPanel.Add(new StackPanel()
            .Spacing(2)
            .Children(
                new Label()
                    .Text(item.Name)
                    .WithTheme((_, label) => label.Foreground(theme.SideBar.Foreground)),
                new Label()
                    .Text(item.Command)
                    .FontSize(11)
                    .WithTheme((_, label) => label.Foreground(theme.SideBar.Foreground))
            ));
    }
}

workbench
    .Theme(t => t
        .SetMode(ThemeVariant.System)
        .SetAccent(Accent.Blue))
    .ActivityBar(bar =>
    {
        bar.Item("all", "全部", GlyphKind.Hamburger, () => ShowCategory("全部"));
        foreach (var category in categories)
        {
            bar.Item(category, category, GlyphKind.Plus, () => ShowCategory(category));
        }
    })
    .SideBar(side => side
        .View("launcher", "启动项", listPanel))
    .EditorArea(editor => editor
        .Document("welcome", "欢迎",
            new StackPanel()
                .Padding(24)
                .Spacing(8)
                .Children(
                    new Label().Text("Mew Launcher").FontSize(24).Bold()
                        .WithTheme((_, label) => label.Foreground(theme.EditorArea.Foreground)),
                    new Label().Text("v0.1.1")
                        .WithTheme((_, label) => label.Foreground(theme.EditorArea.Foreground))
                )))
    .Panel(panel => panel
        .View("output", "输出",
            new StackPanel()
                .Padding(12)
                .Children(
                    new Label().Text("就绪")
                        .WithTheme((_, label) => label.Foreground(theme.Panel.Foreground))
                )))
    .StatusBar(status => status
        .Item("ready", "就绪")
        .Item("shortcut", "Ctrl+Alt+Space"));

ShowCategory("全部");

window.Content = workbench.Build();

Application.Run(window);
