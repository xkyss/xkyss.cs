namespace Mewoo.Abstractions.Theming;

public sealed class MewooTheme
{
    public required string Id { get; init; }

    public required string DisplayName { get; init; }

    public required MewooThemeTokens Tokens { get; init; }

    public Dictionary<string, object> Extensions { get; } = new(StringComparer.Ordinal);
}

public sealed class MewooThemeTokens
{
    public required string WindowBackground { get; init; }

    public required string TitleBarBackground { get; init; }

    public required string ActivityBarBackground { get; init; }

    public required string SidebarBackground { get; init; }

    public required string MainBackground { get; init; }

    public required string PanelBackground { get; init; }

    public required string StatusBarBackground { get; init; }

    public required string TextPrimary { get; init; }

    public required string TextSecondary { get; init; }

    public required string Border { get; init; }

    public required string Accent { get; init; }

    public required string FontFamily { get; init; }

    public required double FontSize { get; init; }

    public required double SpacingUnit { get; init; }
}

public static class MewooBuiltInThemes
{
    public const string DarkId = "mewoo.dark";

    public const string LightId = "mewoo.light";

    public static MewooTheme Dark { get; } = new()
    {
        Id = DarkId,
        DisplayName = "Dark",
        Tokens = new MewooThemeTokens
        {
            WindowBackground = "#1E1E1E",
            TitleBarBackground = "#181818",
            ActivityBarBackground = "#181818",
            SidebarBackground = "#252526",
            MainBackground = "#1E1E1E",
            PanelBackground = "#1E1E1E",
            StatusBarBackground = "#007ACC",
            TextPrimary = "#F3F3F3",
            TextSecondary = "#CCCCCC",
            Border = "#3C3C3C",
            Accent = "#7C3AED",
            FontFamily = "Segoe UI",
            FontSize = 13,
            SpacingUnit = 4,
        },
    };

    public static MewooTheme Light { get; } = new()
    {
        Id = LightId,
        DisplayName = "Light",
        Tokens = new MewooThemeTokens
        {
            WindowBackground = "#FFFFFF",
            TitleBarBackground = "#F3F3F3",
            ActivityBarBackground = "#F3F3F3",
            SidebarBackground = "#F8F8F8",
            MainBackground = "#FFFFFF",
            PanelBackground = "#FFFFFF",
            StatusBarBackground = "#007ACC",
            TextPrimary = "#1F1F1F",
            TextSecondary = "#616161",
            Border = "#D6D6D6",
            Accent = "#7C3AED",
            FontFamily = "Segoe UI",
            FontSize = 13,
            SpacingUnit = 4,
        },
    };
}

