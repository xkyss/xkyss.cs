namespace MewPad.Core.Services;

/// <summary>
/// Service for managing application theme mode (Light/Dark/System).
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Get the current theme.
    /// </summary>
    Theme Current { get; }

    /// <summary>
    /// Observable for theme changes.
    /// </summary>
    IObservable<Theme> Changed { get; }

    /// <summary>
    /// Toggle between Light and Dark theme.
    /// </summary>
    void Toggle();

    /// <summary>
    /// Set theme to a specific value.
    /// </summary>
    void Set(Theme theme);
}

/// <summary>
/// Available application theme modes.
/// </summary>
public enum Theme
{
    /// <summary>Follow operating system theme.</summary>
    System,

    /// <summary>Light theme.</summary>
    Light,

    /// <summary>Dark theme.</summary>
    Dark
}
