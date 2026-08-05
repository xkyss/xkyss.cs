namespace Mew.Launcher;

/// <summary>
/// 数据层纯逻辑(不依赖 UI 与文件):旧格式迁移、分类树子树聚合、未分类聚合、悬空引用归一。
/// 是 LauncherStore 与单元测试共用的唯一逻辑出口。
/// </summary>
public static class LauncherData
{
    /// <summary>
    /// 迁移旧平铺结构:旧分类文本 → 根级分类(按首次出现顺序生成唯一 slug id),「默认」/空分类 → 未分类(null);
    /// 输入为旧模型快照 <see cref="LegacyItem"/>(含分类文本字段)。
    /// </summary>
    public static (List<LauncherCategory> Categories, List<LauncherItem> Items) MigrateLegacy(List<LegacyItem> legacy)
    {
        var categories = new List<LauncherCategory>();
        var byName = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var item in legacy)
        {
            var name = item.Category?.Trim();
            if (string.IsNullOrEmpty(name) || name == "默认")
            {
                continue;
            }

            if (!byName.ContainsKey(name))
            {
                byName[name] = $"cat-{categories.Count + 1}";
                categories.Add(new LauncherCategory(byName[name], name, []));
            }
        }

        var items = legacy
            .Select(item =>
            {
                var name = item.Category?.Trim();
                var categoryId = !string.IsNullOrEmpty(name) && byName.TryGetValue(name, out var id) ? id : null;
                return new LauncherItem(
                    item.Id ?? "item-" + Guid.NewGuid().ToString("N")[..8],
                    item.Name ?? "",
                    item.Command ?? "",
                    item.Args,
                    item.WorkingDirectory,
                    categoryId,
                    item.Icon,
                    item.Hotkey);
            })
            .ToList();

        return (categories, items);
    }

    /// <summary>
    /// 子树聚合:返回归属指定分类及其所有子孙分类的启动项(不含未分类)。
    /// </summary>
    public static List<LauncherItem> AggregateSubtree(
        List<LauncherCategory> categories, List<LauncherItem> items, string categoryId)
    {
        var ids = SubtreeIds(categories, categoryId);
        return items.Where(i => i.CategoryId is not null && ids.Contains(i.CategoryId)).ToList();
    }

    /// <summary>未分类聚合:返回所有 categoryId 为空的启动项。</summary>
    public static List<LauncherItem> Uncategorized(List<LauncherItem> items) =>
        items.Where(i => string.IsNullOrWhiteSpace(i.CategoryId)).ToList();

    /// <summary>归一悬空引用:categoryId 指向不存在的分类 → null(归未分类)。</summary>
    public static List<LauncherItem> NormalizeCategoryRefs(
        List<LauncherCategory> categories, List<LauncherItem> items)
    {
        var known = new HashSet<string>(FlattenIds(categories), StringComparer.Ordinal);
        return items
            .Select(i => i.CategoryId is not null && !known.Contains(i.CategoryId)
                ? i with { CategoryId = null }
                : i)
            .ToList();
    }

    /// <summary>分类树中所有节点 id(扁平,含子孙)。</summary>
    public static IEnumerable<string> FlattenIds(List<LauncherCategory> categories)
    {
        foreach (var category in categories)
        {
            yield return category.Id;
            if (category.Children is not null)
            {
                foreach (var id in FlattenIds(category.Children))
                {
                    yield return id;
                }
            }
        }
    }

    /// <summary>指定分类及其所有子孙的 id 集合。</summary>
    public static HashSet<string> SubtreeIds(List<LauncherCategory> categories, string categoryId)
    {
        var node = Find(categories, categoryId);
        if (node is null)
        {
            return [];
        }

        return new HashSet<string>(FlattenIds([node]), StringComparer.Ordinal);
    }

    /// <summary>按 id 查找分类节点(深度优先,整个树)。</summary>
    public static LauncherCategory? Find(List<LauncherCategory> categories, string categoryId)
    {
        foreach (var category in categories)
        {
            if (category.Id == categoryId)
            {
                return category;
            }

            if (category.Children is not null && Find(category.Children, categoryId) is { } found)
            {
                return found;
            }
        }

        return null;
    }

    /// <summary>分类 id → 显示名(UI 兼容字段填充用);找不到返回 null。</summary>
    public static string? CategoryName(List<LauncherCategory> categories, string? categoryId)
    {
        if (string.IsNullOrEmpty(categoryId))
        {
            return null;
        }

        return Find(categories, categoryId)?.Name;
    }

    /// <summary>按显示名查找分类节点(详情表单分类文本匹配用);找不到返回 null。</summary>
    public static LauncherCategory? FindByName(List<LauncherCategory> categories, string name)
    {
        foreach (var category in categories)
        {
            if (category.Name == name)
            {
                return category;
            }

            if (category.Children is not null && FindByName(category.Children, name) is { } found)
            {
                return found;
            }
        }

        return null;
    }

    /// <summary>归一分类树:Children 为 null 的节点补空表(手写 JSON 缺省)。</summary>
    public static List<LauncherCategory> NormalizeTree(List<LauncherCategory> categories) =>
        categories
            .Select(c => c with { Children = NormalizeTree(c.Children ?? []) })
            .ToList();

    /// <summary>导航树固定节点 id:「全部」与「未分类」。</summary>
    public const string AllNavId = "__all__";
    public const string UncategorizedNavId = "__uncat__";

    /// <summary>构建侧边栏导航树:「全部」置顶、分类树居中(按树结构映射)、「未分类」收尾;固定节点不可管理。</summary>
    public static List<CategoryTreeNode> BuildNavTree(List<LauncherCategory> categories)
    {
        var nodes = categories.Select(ToNavNode).ToList();
        return
        [
            new CategoryTreeNode(AllNavId, "全部", true, []),
            .. nodes,
            new CategoryTreeNode(UncategorizedNavId, "未分类", true, []),
        ];
    }

    /// <summary>按导航节点聚合:null/「全部」→ 所有项;「未分类」→ 无分类项;分类 id → 子树聚合(含子孙)。</summary>
    public static List<LauncherItem> AggregateForNav(
        List<LauncherCategory> categories, List<LauncherItem> items, string? navId)
    {
        if (navId is null or AllNavId)
        {
            return items.ToList();
        }

        return navId == UncategorizedNavId
            ? Uncategorized(items)
            : AggregateSubtree(categories, items, navId);
    }

    private static CategoryTreeNode ToNavNode(LauncherCategory category) => new(
        category.Id,
        category.Name,
        false,
        (category.Children ?? []).Select(ToNavNode).ToList());
}

/// <summary>侧边栏导航树节点:固定节点(「全部」「未分类」)或分类节点(映射自分类树)。</summary>
public sealed record CategoryTreeNode(string Id, string Name, bool IsFixed, List<CategoryTreeNode> Children);

/// <summary>旧模型快照(v0.1.x 平铺数组启动项,含分类文本字段),仅作为迁移输入。</summary>
public sealed record LegacyItem(
    string? Id,
    string? Name,
    string? Command,
    string? Args = null,
    string? WorkingDirectory = null,
    string? Category = null,
    string? Icon = null,
    string? Hotkey = null);
