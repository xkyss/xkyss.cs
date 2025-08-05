using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace Ava.Mvvm.ViewModels;

public partial class MainViewViewModel : ViewModelBase
{
    private readonly IServiceProvider _sp;
    
    public WindowNotificationManager? NotificationManager { get; set; }
    public MenuViewModel Menus { get; init; }

    private object? _content;

    public object? Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    public MainViewViewModel(IServiceProvider sp)
    {
        _sp = sp;
        Menus = _sp.GetRequiredService<MenuViewModel>();
        WeakReferenceMessenger.Default.Register<MainViewViewModel, string>(this, OnNavigation);
        OnSettingsCommand = new RelayCommand(() => OnNavigation(this, "Settings"));
    }


    private void OnNavigation(MainViewViewModel vm, string s)
    {
        Content = s switch
        {
            MenuKeys.MenuKeyIntro => _sp.GetRequiredService<IntroPageViewModel>(),
            MenuKeys.MenuKeySettings => _sp.GetRequiredService<SettingsPageViewModel>(),
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
        };
    }
    
    [ObservableProperty] private bool _isCollapsed;
    
    
    public ICommand OnSettingsCommand { get; set; }
}
