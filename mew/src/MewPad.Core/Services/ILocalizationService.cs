namespace MewPad.Core.Services;

/// <summary>
/// Service for managing application localization (i18n).
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Get the current language code (e.g., 'en-US', 'zh-CN').
    /// </summary>
    string CurrentLanguage { get; }

    /// <summary>
    /// Observable for language changes.
    /// </summary>
    IObservable<string> LanguageChanged { get; }

    /// <summary>
    /// Get a localized string by key.
    /// </summary>
    string GetString(string key, string? section = null);

    /// <summary>
    /// Get a localized string formatted with arguments.
    /// </summary>
    string GetString(string key, params object[] args);

    /// <summary>
    /// Set the current language.
    /// </summary>
    void SetLanguage(string languageCode);

    /// <summary>
    /// Get the list of available languages.
    /// </summary>
    IReadOnlyList<LanguageInfo> AvailableLanguages { get; }
}

/// <summary>
/// Information about an available language.
/// </summary>
public record LanguageInfo(string Code, string Name);
