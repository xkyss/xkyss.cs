namespace MewPad.Core.Services;

using MewPad.Core.Interfaces;

/// <summary>
/// Service for managing application settings.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Register a settings category.
    /// </summary>
    void RegisterCategory(ISettingsCategory category);

    /// <summary>
    /// Get all registered settings categories.
    /// </summary>
    IReadOnlyList<ISettingsCategory> Categories { get; }

    /// <summary>
    /// Get a settings category by ID.
    /// </summary>
    ISettingsCategory? GetCategory(string id);

    /// <summary>
    /// Open the settings panel with a specific category.
    /// </summary>
    void OpenSettings(string categoryId = "appearance");
}