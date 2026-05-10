namespace QuickLaunch.Plugin.Models
{
    /// <summary>
    /// QuickLaunch 完整配置
    /// </summary>
    public class LaunchConfig
    {
        /// <summary>
        /// 配置格式版本
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// 所有分类，按排序顺序
        /// </summary>
        public List<LaunchCategory> Categories { get; set; } = new();

        /// <summary>
        /// 所有启动项，按分类组织
        /// </summary>
        public List<LaunchItem> Items { get; set; } = new();

        /// <summary>
        /// 获取指定分类下的所有启动项（按order排序）
        /// </summary>
        public IEnumerable<LaunchItem> GetItemsByCategory(string categoryId)
        {
            return Items
                .Where(i => i.Category == categoryId && i.Enabled)
                .OrderBy(i => i.Order)
                .ToList();
        }

        /// <summary>
        /// 获取所有一级分类（按order排序）
        /// </summary>
        public IEnumerable<LaunchCategory> GetRootCategories()
        {
            return Categories
                .Where(c => c.ParentId == null && c.Enabled)
                .OrderBy(c => c.Order)
                .ToList();
        }

        /// <summary>
        /// 获取指定一级分类下的所有二级分类（按order排序）
        /// </summary>
        public IEnumerable<LaunchCategory> GetSubCategories(string parentId)
        {
            return Categories
                .Where(c => c.ParentId == parentId && c.Enabled)
                .OrderBy(c => c.Order)
                .ToList();
        }

        /// <summary>
        /// 根据ID查找分类
        /// </summary>
        public LaunchCategory? FindCategory(string categoryId)
        {
            return Categories.FirstOrDefault(c => c.Id == categoryId);
        }
    }
}
