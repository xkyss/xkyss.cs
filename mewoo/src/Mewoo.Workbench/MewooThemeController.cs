using Aprillz.MewUI;
using Mewoo.Abstractions.Theming;

namespace Mewoo.Workbench;

public sealed class MewooThemeController
{
    public event Action? Changed;

    public MewooTheme CurrentTheme { get; private set; } = MewooBuiltInThemes.Dark;

    public IReadOnlyList<MewooTheme> BuiltInThemes { get; } =
    [
        MewooBuiltInThemes.Dark,
        MewooBuiltInThemes.Light,
    ];

    public void Apply(string themeId)
    {
        CurrentTheme = themeId == MewooBuiltInThemes.LightId
            ? MewooBuiltInThemes.Light
            : MewooBuiltInThemes.Dark;

        if (Application.IsRunning)
        {
            Application.Current.SetTheme(CurrentTheme.Id == MewooBuiltInThemes.LightId
                ? ThemeVariant.Light
                : ThemeVariant.Dark);
        }

        Changed?.Invoke();
    }

    public void Toggle()
    {
        Apply(CurrentTheme.Id == MewooBuiltInThemes.DarkId
            ? MewooBuiltInThemes.LightId
            : MewooBuiltInThemes.DarkId);
    }
}

