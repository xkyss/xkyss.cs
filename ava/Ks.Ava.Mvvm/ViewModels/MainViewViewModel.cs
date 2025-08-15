using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Ks.Ava.Mvvm.Base.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace Ks.Ava.Mvvm.ViewModels;

public partial class MainViewViewModel : ViewModelBase
{
    private readonly IServiceProvider _sp;
    
    public WindowNotificationManager? NotificationManager { get; set; }
    public MenuViewModel Menus { get; }

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
        WeakReferenceMessenger.Default.Register<MainViewViewModel, Type>(this, OnNavigation);
    }


    private void OnNavigation(MainViewViewModel @this, Type type)
    {
        Content = _sp.GetRequiredService(type);
    }
    
    [ObservableProperty] private bool _isCollapsed;
    
}
