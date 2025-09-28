using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ks.Ava.Mvvm.Base.Plugin;
using Ks.Ava.Mvvm.Base.ViewModels;

namespace Ks.Ava.Mvvm.ViewModels;

public class MenuViewModel : ViewModelBase
{
    public MenuViewModel(IEnumerable<IPlugin> plugins)
    {
        foreach (var plugin in plugins)
        {
            MenuItems.Add(new MenuItemViewModel()
            {
                MenuHeader = plugin.Name, 
                Key = plugin.Name, 
                ViewModelType = plugin.ViewModelType
            });
        }
    }

    public ObservableCollection<MenuItemViewModel> MenuItems { get; set; } = [];
}
