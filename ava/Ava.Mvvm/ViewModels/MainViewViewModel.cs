using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        OnSettingsCommand = new RelayCommand(() => OnNavigation(this, "Settings"));
    }


    private void OnNavigation(MainViewViewModel vm, string s)
    {
        Content = s switch
        {
            MenuKeys.MenuKeyIntro => new IntroPageViewModel(),
            MenuKeys.MenuKeySettings => new SettingsPageViewModel(),
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
        };
    }
    
    [ObservableProperty] private bool _isCollapsed;
    
    
    public ICommand OnSettingsCommand { get; set; }
}
