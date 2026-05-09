namespace MewPad.Core.Interfaces;

using Aprillz.MewUI.Controls;

/// <summary>
/// Represents a content tab item in the ContentArea.
/// </summary>
public interface IContentItem
{
    /// <summary>
    /// Unique identifier for this content tab.
    /// Used to reuse the same tab if opened multiple times.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Display title for the content tab.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Icon to display in content tab (glyph or ImageSource).
    /// </summary>
    object? Icon { get; }

    /// <summary>
    /// Whether this tab can be closed by the user.
    /// </summary>
    bool CanClose => true;

    /// <summary>
    /// Creates the content element for this tab.
    /// </summary>
    FrameworkElement CreateContent();
}
