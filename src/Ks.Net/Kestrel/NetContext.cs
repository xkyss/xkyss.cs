using Microsoft.AspNetCore.Http.Features;

namespace Ks.Net.Kestrel;


/// <summary>
/// 表示应用程序请求上下文
/// </summary>
public abstract class NetContext
{
    /// <summary>
    /// 获取特征集合
    /// </summary>
    public IFeatureCollection? Features { get; }

    /// <summary>
    /// 应用程序请求上下文
    /// </summary>
    /// <param name="features"></param>
    public NetContext(IFeatureCollection? features)
    {
        if (features != null)
        {
            Features = new FeatureCollection(features);
        }
    }
}
