namespace MewPad.Hosting.Extensions;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewPad.Core.Interfaces;

public class AboutSettings : ISettingsCategory
{
    public string Id => "about";
    public string Title => "About";
    public object? Icon => "ℹ";
    public int Order => 99;

    public FrameworkElement CreateView() =>
        new StackPanel().Vertical().Children(
            new Label { Text = "About MewPad", FontWeight = FontWeight.Bold, FontSize = 16, Margin = new Thickness(0, 0, 0, 16) },
            new Label { Text = "MewPad Universal Desktop Shell Framework", Margin = new Thickness(0, 0, 0, 8) },
            new Label { Text = "Version: 0.1.0-dev", Margin = new Thickness(0, 0, 0, 4) },
            new Label { Text = "Runtime: .NET 10 + Aprillz MewUI", Margin = new Thickness(0, 0, 0, 4) },
            new Label { Text = "License: MIT", Margin = new Thickness(0, 0, 0, 16) },
            new Label { Text = "Built with ❤ using code-first UI (no XAML).", FontSize = 11 }
        );
}
