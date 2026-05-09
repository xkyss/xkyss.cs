namespace MewPad.Core.Services.Impl;

using MewPad.Core.Services;

internal class ThemeService : IThemeService
{
    private readonly ObservableValue<Theme> _current = new(Theme.Dark);

    public Theme Current => _current.Value;
    public IObservable<Theme> Changed => _current.Changed;

    public void Toggle() => Set(Current == Theme.Light ? Theme.Dark : Theme.Light);
    public void Set(Theme theme) => _current.Value = theme;
}