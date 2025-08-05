using System.Collections.ObjectModel;

namespace Ava.Mvvm.ViewModels;

public class MenuViewModel : ViewModelBase
{
    public MenuViewModel()
    {
        MenuItems = new ObservableCollection<MenuItemViewModel>
        {
            new() { MenuHeader = "Intro", Key = MenuKeys.MenuKeyIntro, IsSeparator = false },
        };
    }

    public ObservableCollection<MenuItemViewModel> MenuItems { get; set; }
}

public static class MenuKeys
{
    public const string MenuKeyIntro = "Intro";
}