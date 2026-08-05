using Aprillz.MewUI;

namespace Mew.Workbench;

/// <summary>
/// Identifies one of the five Workbench zones.
/// </summary>
public enum WorkbenchZone
{
    ActivityBar,
    SideBar,
    EditorArea,
    Panel,
    StatusBar,
}

/// <summary>
/// Zone-specific colors read by Workbench zones and tool views.
/// </summary>
public sealed record ZonePalette(Color Background, Color Foreground, Color Accent)
{
    internal static ZonePalette Create(Theme theme, WorkbenchZone zone)
    {
        var palette = theme.Palette;

        return zone switch
        {
            WorkbenchZone.ActivityBar => new ZonePalette(palette.ControlBackground, palette.WindowText, palette.Accent),
            WorkbenchZone.SideBar => new ZonePalette(palette.ContainerBackground, palette.WindowText, palette.Accent),
            WorkbenchZone.EditorArea => new ZonePalette(palette.WindowBackground, palette.WindowText, palette.Accent),
            WorkbenchZone.Panel => new ZonePalette(palette.ContainerBackground, palette.WindowText, palette.Accent),
            WorkbenchZone.StatusBar => new ZonePalette(palette.ControlBackground, palette.WindowText, palette.Accent),
            _ => throw new ArgumentOutOfRangeException(nameof(zone)),
        };
    }
}

/// <summary>
/// Theme context that maps the active MewUI theme to Workbench zone palettes.
/// </summary>
public sealed class WorkbenchThemeContext
{
    internal WorkbenchThemeContext()
    {
    }

    public ThemeVariant Mode => Application.IsRunning ? Application.Current!.ThemeMode : ThemeManager.Default;

    public bool IsDark => CurrentTheme.IsDark;

    public Color Accent => CurrentTheme.Palette.Accent;

    public ZonePalette ActivityBar => Get(WorkbenchZone.ActivityBar);

    public ZonePalette SideBar => Get(WorkbenchZone.SideBar);

    public ZonePalette EditorArea => Get(WorkbenchZone.EditorArea);

    public ZonePalette Panel => Get(WorkbenchZone.Panel);

    public ZonePalette StatusBar => Get(WorkbenchZone.StatusBar);

    public ZonePalette Get(WorkbenchZone zone) => ZonePalette.Create(CurrentTheme, zone);

    public WorkbenchThemeContext SetMode(ThemeVariant mode)
    {
        if (Application.IsRunning)
        {
            Application.Current!.SetThemeMode(mode);
        }
        else
        {
            ThemeManager.Default = mode;
        }

        return this;
    }

    public WorkbenchThemeContext SetAccent(Accent accent)
    {
        if (Application.IsRunning)
        {
            Application.Current!.SetAccent(accent);
        }
        else
        {
            ThemeManager.DefaultAccent = accent;
        }

        return this;
    }

    public WorkbenchThemeContext SetAccent(Color accent)
    {
        if (Application.IsRunning)
        {
            Application.Current!.SetAccent(accent);
        }
        else
        {
            ThemeManager.DefaultAccentColor = accent;
        }

        return this;
    }

    private static Theme CurrentTheme => Application.IsRunning ? Application.Current!.Theme : CreateFallbackTheme();

    internal static Theme CreateFallbackTheme()
    {
        var isDark = ThemeManager.Default == ThemeVariant.Dark;
        var seed = isDark ? ThemeManager.DefaultDarkSeed : ThemeManager.DefaultLightSeed;
        var accent = ThemeManager.DefaultAccentColor ?? ThemeManager.DefaultAccent.GetAccentColor(isDark);

        return new Theme
        {
            Name = isDark ? "Dark" : "Light",
            Palette = new Palette(seed, accent),
            Metrics = ThemeManager.DefaultMetrics,
        };
    }
}
