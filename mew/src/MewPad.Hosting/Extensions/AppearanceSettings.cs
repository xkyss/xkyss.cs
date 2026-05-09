namespace MewPad.Hosting.Extensions;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewPad.Core;
using MewPad.Core.Interfaces;
using MewPad.Core.Services;
using AppTheme = MewPad.Core.Services.Theme;

public class AppearanceSettings : ISettingsCategory
{
    private readonly IThemeService _theme;

    public string Id => "appearance";
    public string Title => "Appearance";
    public object? Icon => "🎨";
    public int Order => 1;

    public AppearanceSettings(IThemeService theme) => _theme = theme;

    public FrameworkElement CreateView()
    {
        var currentLabel = new Label
        {
            Text = $"Current theme: {_theme.Current}",
            Margin = new Thickness(0, 0, 0, 12),
        };

        // Update label when theme changes
        _theme.Changed.Subscribe(t => currentLabel.Text = $"Current theme: {t}");

        var lightBtn = new Button { Content = new Label { Text = "☀️  Light" }, MinWidth = 100, Margin = new Thickness(0, 0, 8, 0) };
        var darkBtn  = new Button { Content = new Label { Text = "🌙  Dark" },  MinWidth = 100 };

        lightBtn.Click += () => _theme.Set(AppTheme.Light);
        darkBtn.Click  += () => _theme.Set(AppTheme.Dark);

        return new StackPanel().Vertical().Children(
            new Label { Text = "Theme", FontWeight = FontWeight.Bold, FontSize = 16, Margin = new Thickness(0, 0, 0, 12) },
            currentLabel,
            new StackPanel().Horizontal().Children(lightBtn, darkBtn)
        );
    }
}