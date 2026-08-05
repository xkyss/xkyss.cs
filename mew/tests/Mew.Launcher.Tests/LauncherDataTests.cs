using Mew.Launcher;
using Xunit;

namespace Mew.Launcher.Tests;

/// <summary>
/// 数据层纯逻辑:旧格式迁移、分类树子树聚合、未分类聚合、悬空引用归一。
/// </summary>
public class LauncherDataTests
{
    // ── 迁移 ──────────────────────────────────────────────

    [Fact]
    public void MigrateLegacy_中文分类_生成根级分类并引用()
    {
        var legacy = new List<LauncherItem>
        {
            new("vs-code", "VS Code", "code", Category: "开发"),
            new("steam", "Steam", "steam", Category: "游戏"),
            new("github", "GitHub", "https://github.com", Category: "工具"),
        };

        var (categories, items) = LauncherData.MigrateLegacy(legacy);

        Assert.Equal(3, categories.Count);
        Assert.Equal("开发", categories[0].Name);
        Assert.Equal("游戏", categories[1].Name);
        Assert.Equal("工具", categories[2].Name);
        Assert.All(categories, c => Assert.Matches("^[a-z0-9-]+$", c.Id)); // 唯一 slug
        Assert.Equal(categories[0].Id, items[0].CategoryId);
        Assert.Equal(categories[1].Id, items[1].CategoryId);
        Assert.Equal(categories[2].Id, items[2].CategoryId);
    }

    [Fact]
    public void MigrateLegacy_默认与空分类_归未分类()
    {
        var legacy = new List<LauncherItem>
        {
            new("a", "A", "a", Category: "默认"),
            new("b", "B", "b", Category: ""),
        };

        var (categories, items) = LauncherData.MigrateLegacy(legacy);

        Assert.Empty(categories);
        Assert.All(items, i => Assert.Null(i.CategoryId));
    }

    [Fact]
    public void MigrateLegacy_空列表_产出空结构()
    {
        var (categories, items) = LauncherData.MigrateLegacy([]);

        Assert.Empty(categories);
        Assert.Empty(items);
    }

    [Fact]
    public void MigrateLegacy_重复分类_只建一次()
    {
        var legacy = new List<LauncherItem>
        {
            new("a", "A", "a", Category: "开发"),
            new("b", "B", "b", Category: "开发"),
        };

        var (categories, items) = LauncherData.MigrateLegacy(legacy);

        Assert.Single(categories);
        Assert.All(items, i => Assert.Equal(categories[0].Id, i.CategoryId));
    }

    // ── 聚合 ──────────────────────────────────────────────

    [Fact]
    public void AggregateSubtree_父分类_返回子孙所有项()
    {
        var categories = new List<LauncherCategory>
        {
            new("games", "游戏", [new LauncherCategory("games-steam", "Steam", [])]),
        };
        var items = new List<LauncherItem>
        {
            new("launcher", "启动器", "mew", CategoryId: "games"),
            new("steam", "Steam", "steam", CategoryId: "games-steam"),
            new("github", "GitHub", "https://github.com", CategoryId: null),
        };

        var result = LauncherData.AggregateSubtree(categories, items, "games");

        Assert.Equal(2, result.Count);
        Assert.Contains(result, i => i.Id == "launcher");
        Assert.Contains(result, i => i.Id == "steam");
    }

    [Fact]
    public void AggregateSubtree_叶子分类_只返回直接项()
    {
        var categories = new List<LauncherCategory>
        {
            new("games", "游戏", [new LauncherCategory("games-steam", "Steam", [])]),
        };
        var items = new List<LauncherItem>
        {
            new("launcher", "启动器", "mew", CategoryId: "games"),
            new("steam", "Steam", "steam", CategoryId: "games-steam"),
        };

        var result = LauncherData.AggregateSubtree(categories, items, "games-steam");

        Assert.Single(result);
        Assert.Equal("steam", result[0].Id);
    }

    [Fact]
    public void Uncategorized_返回categoryId为空的项()
    {
        var items = new List<LauncherItem>
        {
            new("a", "A", "a", CategoryId: null),
            new("b", "B", "b", CategoryId: "games"),
        };

        var result = LauncherData.Uncategorized(items);

        Assert.Single(result);
        Assert.Equal("a", result[0].Id);
    }

    // ── 悬空归一 ──────────────────────────────────────────

    [Fact]
    public void NormalizeCategoryRefs_悬空引用_归未分类()
    {
        var categories = new List<LauncherCategory>
        {
            new("games", "游戏", []),
        };
        var items = new List<LauncherItem>
        {
            new("a", "A", "a", CategoryId: "games"),
            new("b", "B", "b", CategoryId: "ghost"),
        };

        var result = LauncherData.NormalizeCategoryRefs(categories, items);

        Assert.Equal("games", result[0].CategoryId);
        Assert.Null(result[1].CategoryId);
    }
}
