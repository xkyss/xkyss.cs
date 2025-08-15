using System.Collections.ObjectModel;
using Ks.Ava.Mvvm.Base.ViewModels;

namespace Ks.Ava.Mvvm.ViewModels;

public class MenuViewModel : ViewModelBase
{
    public MenuViewModel()
    {
        MenuItems = new ObservableCollection<MenuItemViewModel>
        {
            new() { MenuHeader = "Intro", Key = MenuKeys.MenuKeyIntro, IsSeparator = false },
            new() { MenuHeader = "Settings", Key = MenuKeys.MenuKeySettings, IsSeparator = false },
        };
    }

    public ObservableCollection<MenuItemViewModel> MenuItems { get; set; }
}

public static class MenuKeys
{
    public const string MenuKeyIntro = "Intro";
    public const string MenuKeySettings = "Settings";
}