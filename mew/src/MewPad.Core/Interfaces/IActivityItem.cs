namespace MewPad.Core.Interfaces;

using Aprillz.MewUI.Controls;

/// <summary>
/// Represents an activity item in the ActivityBar (sidebar navigation).
/// </summary>
public interface IActivityItem
{
    /// <summary>
    /// Unique identifier for this activity.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Icon to display in ActivityBar (glyph character or ImageSource).
    /// </summary>
    object Icon { get; }

    /// <summary>
    /// Display title/label for this activity.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Section where this activity appears in ActivityBar.
    /// </summary>
    ActivityBarSection Section => ActivityBarSection.Top;

    /// <summary>
    /// Sort order (lower values appear first).
    /// </summary>
    int Order => 100;

    /// <summary>
    /// Creates the content element for the SideBar when this activity is active.
    /// </summary>
    FrameworkElement CreateContent();
}

/// <summary>
/// Sections of the ActivityBar.
/// </summary>
public enum ActivityBarSection
{
    /// <summary>Top section - main navigation items.</summary>
    Top,
    
    /// <summary>Bottom section - global items like Settings.</summary>
    Bottom
}
