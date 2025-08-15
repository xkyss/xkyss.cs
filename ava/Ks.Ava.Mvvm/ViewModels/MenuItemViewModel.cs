using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Ks.Ava.Mvvm.Base.ViewModels;

namespace Ks.Ava.Mvvm.ViewModels;

public class MenuItemViewModel: ViewModelBase
{
    public string? MenuHeader { get; set; }
    public string? MenuIconName { get; set; }
    public string? Key { get; set; }
    
    public Type? ViewModelType { get; set; }
    public string? Status { get; set; }
    
    public bool IsSeparator { get; set; }
    public ObservableCollection<MenuItemViewModel> Children { get; set; } = new();
    
    public ICommand ActivateCommand { get; set; }

    public MenuItemViewModel()
    {
        ActivateCommand = new RelayCommand(OnActivate);
    }

    private void OnActivate()
    {
        if (IsSeparator || ViewModelType is null)
        {
            return;
        }
        WeakReferenceMessenger.Default.Send(ViewModelType);
    }
}