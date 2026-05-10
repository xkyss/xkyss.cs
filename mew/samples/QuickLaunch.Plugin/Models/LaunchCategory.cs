namespace QuickLaunch.Plugin.Models
{
    /// <summary>
    /// 启动项分类（一级或二级）
    /// </summary>
    public class LaunchCategory
    {
        /// <summary>
        /// 分类唯一标识符
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 显示名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 图标（emoji或图标代码，可选）
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// 父分类ID（一级分类为空，二级分类指向父一级分类ID）
        /// </summary>
        public string? ParentId { get; set; }

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
