namespace MewPad.Hosting.Extensions;

using Aprillz.MewUI.Controls;
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

    public FrameworkElement CreateView() =>
        new Label().Text("Language: English / 简体中文");
}