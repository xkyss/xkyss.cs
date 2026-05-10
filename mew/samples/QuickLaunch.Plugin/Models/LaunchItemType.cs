namespace QuickLaunch.Plugin.Models
{
    /// <summary>
    /// 启动项类型
    /// </summary>
    public enum LaunchItemType
    {
        /// <summary>
        /// 网页（通过浏览器打开URL）
        /// </summary>
        Web,

        /// <summary>
        /// 应用程序（执行本地exe）
        /// </summary>
        Application,

        /// <summary>
        /// 脚本/命令（在shell中执行命令）
        /// </summary>
        Script
    }
}
