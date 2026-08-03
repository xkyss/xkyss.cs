using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Mew.Workbench;

Win32Platform.Register();
Direct2DBackend.Register();

var window = new Window()
    .Title("Mew Launcher — v0.1.1")
    .Resizable(1080, 720);

var workbench = new Workbench();
workbench
    .Theme(theme => theme
        .SetMode(ThemeVariant.System)
        .SetAccent(Accent.Blue))
    .ActivityBar(bar => bar
        .Item("launcher", "启动项", GlyphKind.Hamburger)
        .Item("new", "新建", GlyphKind.Plus))
    .SideBar(side => side
        .View("launcher", "启动项",
            new StackPanel()
                .Padding(12)
                .Spacing(6)
                .Children(
                    new Label().Text("启动项").FontSize(14).Bold()
                        .WithTheme((_, label) => label.Foreground(workbench.ThemeContext.SideBar.Foreground)),
                    new Label().Text("VS Code")
                        .WithTheme((_, label) => label.Foreground(workbench.ThemeContext.SideBar.Foreground)),
                    new Label().Text("Terminal")
                        .WithTheme((_, label) => label.Foreground(workbench.ThemeContext.SideBar.Foreground)),
                    new Label().Text("GitHub")
                        .WithTheme((_, label) => label.Foreground(workbench.ThemeContext.SideBar.Foreground))
                )))
    .EditorArea(editor => editor
        .Document("welcome", "欢迎",
            new StackPanel()
                .Padding(24)
                .Spacing(8)
                .Children(
                    new Label().Text("Mew Launcher").FontSize(24).Bold()
                        .WithTheme((_, label) => label.Foreground(workbench.ThemeContext.EditorArea.Foreground)),
                    new Label().Text("v0.1.1")
                        .WithTheme((_, label) => label.Foreground(workbench.ThemeContext.EditorArea.Foreground))
                )))
    .Panel(panel => panel
        .View("output", "输出",
            new StackPanel()
                .Padding(12)
                .Children(
                    new Label().Text("就绪")
                        .WithTheme((_, label) => label.Foreground(workbench.ThemeContext.Panel.Foreground))
                )))
    .StatusBar(status => status
        .Item("ready", "就绪")
        .Item("shortcut", "Ctrl+Alt+Space"));

window.Content = workbench.Build();

Application.Run(window);
