namespace MewPad.Core.Interfaces;

using Aprillz.MewUI.Controls;

/// <summary>
/// Represents a panel item in the PanelArea (bottom panel).
/// </summary>
public interface IPanelItem
{
    /// <summary>
    /// Unique identifier for this panel.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Display title for the panel tab.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Icon to display in panel tab (glyph or ImageSource).
    /// </summary>
    object? Icon { get; }

    /// <summary>
    /// Sort order (lower values appear first).
    /// </summary>
    int Order => 100;

    /// <summary>
    /// Creates the content element for this panel.
    /// </summary>
    FrameworkElement CreateContent();
}
