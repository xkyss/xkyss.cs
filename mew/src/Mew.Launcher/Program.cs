using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

Win32Platform.Register();
Direct2DBackend.Register();

var window = new Window()
    .Title("Mew Launcher")
    .Resizable(520, 360)
    .Padding(12)
    .Content(
        new StackPanel()
            .Spacing(8)
            .Children(
                new Label()
                    .Text("Mew Launcher — v0.1.1 骨架")
                    .FontSize(18)
            )
    );

Application.Run(window);
