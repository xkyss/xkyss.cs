namespace MewPad.Hosting.Extensions;

using Aprillz.MewUI.Controls;
using MewPad.Core.Interfaces;
using MewPad.Core.Services;

public class AppearanceSettings : ISettingsCategory
{
    private readonly IThemeService _theme;

    public string Id => "appearance";
    public string Title => "Appearance";
    public object? Icon => "🎨";
    public int Order => 1;

    public AppearanceSettings(IThemeService theme) => _theme = theme;

    public FrameworkElement CreateView() =>
        new Label().Text("Theme: Light/Dark");
}