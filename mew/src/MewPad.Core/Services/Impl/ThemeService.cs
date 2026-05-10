namespace MewPad.Core.Services.Impl;

using MewPad.Core.Services;

internal class ThemeService : IThemeService
{
    private readonly ObservableValue<Theme> _current = new(Theme.System);

    public Theme Current => _current.Value;
    public IObservable<Theme> Changed => _current.Changed;

    public void Toggle()
    {
        var next = Current switch
        {
            Theme.System => Theme.Light,
            Theme.Light => Theme.Dark,
            _ => Theme.System,
        };

        Set(next);
    }

    public void Set(Theme theme) => _current.Value = theme;
}