namespace QuickLaunch.Plugin.Models
{
    /// <summary>
    /// 启动项（网页、应用、脚本）
    /// </summary>
    public class LaunchItem
    {
        /// <summary>
        /// 唯一标识符（kebab-case）
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 显示名称（可包含emoji）
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 启动项类型
        /// </summary>
        public LaunchItemType Type { get; set; }

        /// <summary>
        /// 启动目标：
        /// - Web: URL (如 https://github.com)
        /// - Application: exe路径 (如 C:\Program Files\App\app.exe)
        /// - Script: 命令行命令 (如 npm run dev)
        /// </summary>
        public string Target { get; set; } = string.Empty;

        /// <summary>
        /// 父分类ID
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 图标（emoji或图标代码）
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// 简短描述（可选）
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 排序优先级（较小值优先显示）
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;
    }
}
