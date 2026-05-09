namespace MewPad.Core.Interfaces;

using Aprillz.MewUI.Controls;

/// <summary>
/// Represents a settings category in the Settings panel.
/// </summary>
public interface ISettingsCategory
{
    /// <summary>
    /// Unique identifier for this settings category.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Display title for the settings category.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Icon to display in category list (glyph or ImageSource).
    /// </summary>
    object? Icon { get; }

    /// <summary>
    /// Sort order (lower values appear first).
    /// </summary>
    int Order => 100;

    /// <summary>
    /// Creates the settings view for this category.
    /// </summary>
    FrameworkElement CreateView();
}
