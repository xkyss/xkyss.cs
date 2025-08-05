using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Ursa.Themes.Semi;
using Notification = Ursa.Controls.Notification;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace Ava.Mvvm.ViewModels;

public partial class MainViewViewModel : ViewModelBase
{
    public WindowNotificationManager? NotificationManager { get; set; }
    public MenuViewModel Menus { get; set; } = new MenuViewModel();

    private object? _content;

    public object? Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    public MainViewViewModel()
    {
        WeakReferenceMessenger.Default.Register<MainViewViewModel, string>(this, OnNavigation);
    }


    private void OnNavigation(MainViewViewModel vm, string s)
    {
        Content = s switch
        {
            MenuKeys.MenuKeyIntro => new IntroPageViewModel(),
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
        };
    }

    public ObservableCollection<ThemeItem> Themes { get; } =
    [
        new("Default", ThemeVariant.Default),
        new("Light", ThemeVariant.Light),
        new("Dark", ThemeVariant.Dark),
        new("Aquatic", SemiTheme.Aquatic),
        new("Desert", SemiTheme.Desert),
        new("Dusk", SemiTheme.Dusk),
        new("NightSky", SemiTheme.NightSky)
    ];

    [ObservableProperty] private ThemeItem? _selectedTheme;

    partial void OnSelectedThemeChanged(ThemeItem? oldValue, ThemeItem? newValue)
    {
        if (newValue is null) return;
        var app = Application.Current;
        if (app is not null)
        {
            app.RequestedThemeVariant = newValue.Theme;
            NotificationManager?.Show(
                new Notification("Theme changed", $"Theme changed to {newValue.Name}"),
                type: NotificationType.Success,
                classes: ["Light"]);
        }
    }

    [ObservableProperty] private string? _footerText = "Settings";

    [ObservableProperty] private bool _isCollapsed;

    partial void OnIsCollapsedChanged(bool value)
    {
        FooterText = value ? null : "Settings";
    }
}

public class ThemeItem(string name, ThemeVariant theme)
{
    public string Name { get; set; } = name;
    public ThemeVariant Theme { get; set; } = theme;
}