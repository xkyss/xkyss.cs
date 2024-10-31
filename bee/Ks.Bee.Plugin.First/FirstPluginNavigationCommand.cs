using Ks.Bee.Base.Abstractions.Navigation;
using Ks.Bee.Base.Models.Navigation;
using Ks.Bee.Plugin.First.ViewModels;

namespace Ks.Bee.Plugin.First;

public class FirstPluginNavigationCommand : INavigationCommand
{
    public string Key => "First";

    private readonly FirstViewModel _vm;

    public FirstPluginNavigationCommand(FirstViewModel firstViewModel)
    {
        _vm = firstViewModel;
    }

    public void Execute(NavigationCommandContext context)
    {
        context.Navigator?.SetCurrentPage(_vm);
    }
}
