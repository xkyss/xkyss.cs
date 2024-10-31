using Ks.Bee.Base.Abstractions.Navigation;
using Ks.Bee.Base.Models.Navigation;
using Ks.Bee.ViewModels.Images;

namespace Ks.Bee.Services.Impl.Navigation.Commands;

/// <summary>
/// 海报生成器导航命令
/// </summary>
public class PosterGeneratorNavigationCommand : INavigationCommand
{
    public string Key => "PosterGenerator";

    /// <summary>
    /// 执行导航
    /// </summary>
    /// <param name="context"></param>
    public void Execute(NavigationCommandContext context)
    {
        // 调用视图导航器设置当前页面
        context.Navigator?.SetCurrentPage(new PosterGeneratorViewModel());
    }
}
