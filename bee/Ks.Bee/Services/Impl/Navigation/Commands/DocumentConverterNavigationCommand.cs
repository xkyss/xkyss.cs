
using Ks.Bee.Models.Navigation;
using Ks.Bee.Services.Abstractions.Navigation;
using Ks.Bee.ViewModels.Documents;

namespace Ks.Bee.Services.Impl.Navigation.Commands;

/// <summary>
/// 文档格式转换导航命令
/// </summary>
public class DocumentConverterNavigationCommand : INavigationCommand
{
    public void Execute(NavigationCommandContext context)
    {
        context.Navigator?.SetCurrentPage(new DocumentConverterViewModel());
    }
}
