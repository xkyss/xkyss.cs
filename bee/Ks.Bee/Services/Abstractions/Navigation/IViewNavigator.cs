
using System.ComponentModel;
using Ks.Bee.Models.Navigation;
using Ks.Bee.ViewModels;

namespace Ks.Bee.Services.Abstractions.Navigation;

/// <summary>
/// 视图导航器接口
/// </summary>
public interface IViewNavigator
{
    /// <summary>
    /// 属性变化通知事件
    /// </summary>
    event PropertyChangedEventHandler PropertyChanged;
    /// <summary>
    /// 当前页视图模型对象
    /// </summary>
    PageViewModelBase? CurrentPage { get; }
    /// <summary>
    /// 设置当前页
    /// </summary>
    /// <param name="page"></param>
    void SetCurrentPage(PageViewModelBase page);
    /// <summary>
    /// 导航到视图
    /// </summary>
    /// <param name="page"></param>
    void NavigateTo(ViewPages page);
}
