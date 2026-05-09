namespace MewPad.Core.Services.Impl;

using MewPad.Core.Services;

internal class LocalizationService : ILocalizationService
{
    private readonly ObservableValue<string> _currentLanguage = new("en-US");
    private readonly Dictionary<string, LanguageInfo> _languages = new()
    {
        ["en-US"] = new LanguageInfo("en-US", "English"),
        ["zh-CN"] = new LanguageInfo("zh-CN", "简体中文")
    };
    private readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> _translations = [];

    public string CurrentLanguage => _currentLanguage.Value;
    public IObservable<string> LanguageChanged => _currentLanguage.Changed;
    public IReadOnlyList<LanguageInfo> AvailableLanguages => _languages.Values.ToList();

    public LocalizationService()
    {
        _translations["en-US"] = new()
        {
            ["ui"] = new()
            {
                ["welcome"] = "Welcome to MewPad",
                ["settings"] = "Settings",
                ["appearance"] = "Appearance",
                ["language"] = "Language",
                ["theme"] = "Theme",
                ["light"] = "Light",
                ["dark"] = "Dark"
            }
        };
        _translations["zh-CN"] = new()
        {
            ["ui"] = new()
            {
                ["welcome"] = "欢迎使用 MewPad",
                ["settings"] = "设置",
                ["appearance"] = "外观",
                ["language"] = "语言",
                ["theme"] = "主题",
                ["light"] = "浅色",
                ["dark"] = "深色"
            }
        };
    }

    public string GetString(string key, string? section = null)
    {
        var lang = CurrentLanguage;
        if (!_translations.ContainsKey(lang)) lang = "en-US";
        var sectionKey = section ?? "ui";
        var dict = _translations[lang];
        return dict.TryGetValue(sectionKey, out var sec) && sec.TryGetValue(key, out var val) ? val : key;
    }

    public string GetString(string key, params object[] args)
    {
        try { return string.Format(GetString(key), args); }
        catch { return key; }
    }

    public void SetLanguage(string languageCode)
    {
        if (_languages.ContainsKey(languageCode))
            _currentLanguage.Value = languageCode;
    }
}