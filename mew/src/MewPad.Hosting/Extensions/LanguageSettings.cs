namespace MewPad.Hosting.Extensions;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewPad.Core;
using MewPad.Core.Interfaces;
using MewPad.Core.Services;

public class LanguageSettings : ISettingsCategory
{
    private readonly ILocalizationService _localization;

    public string Id => "language";
    public string Title => "Language";
    public object? Icon => "🌐";
    public int Order => 2;

    public LanguageSettings(ILocalizationService localization) => _localization = localization;

    public FrameworkElement CreateView()
    {
        var currentLabel = new Label
        {
            Text = $"Current language: {_localization.CurrentLanguage}",
            Margin = new Thickness(0, 0, 0, 12),
        };

        _localization.LanguageChanged.Subscribe(lang =>
            currentLabel.Text = $"Current language: {lang}");

        var btnStack = new StackPanel().Vertical();
        foreach (var langInfo in _localization.AvailableLanguages)
        {
            var info = langInfo;
            var btn = new Button
            {
                Content = new Label { Text = $"{info.Name}  ({info.Code})" },
                MinWidth = 200,
                Margin = new Thickness(0, 4),
            };
            btn.Click += () => _localization.SetLanguage(info.Code);
            btnStack.Children(btn);
        }

        return new StackPanel().Vertical().Children(
            new Label { Text = "Language", FontWeight = FontWeight.Bold, FontSize = 16, Margin = new Thickness(0, 0, 0, 12) },
            currentLabel,
            btnStack
        );
    }
}