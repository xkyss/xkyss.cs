using System.Text.Json;
using Mew.Launcher;
using Xunit;

namespace Mew.Launcher.Tests;

/// <summary>
/// 数据文件读写:新结构往返、手写新格式可识别、旧格式首启迁移并写回新格式。
/// </summary>
public class LauncherStoreTests : IDisposable
{
    private readonly string _dir;

    public LauncherStoreTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "mew-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_dir, true);
        }
        catch (IOException)
        {
        }
    }

    private string TempFile(string name) => Path.Combine(_dir, name);

    [Fact]
    public void SaveThenLoad_新结构往返一致()
    {
        var store = new LauncherStore(TempFile("launcher.json"));
        store.UpdateCategories(new List<LauncherCategory>
        {
            new("games", "游戏", [new LauncherCategory("games-steam", "Steam", [])]),
        });
        var items = new List<LauncherItem>
        {
            new("steam", "Steam", "steam", CategoryId: "games-steam"),
            new("github", "GitHub", "https://github.com", CategoryId: null),
        };
        store.Save(items);

        var reloaded = new LauncherStore(TempFile("launcher.json"));
        var loaded = reloaded.Load();

        Assert.Equal(2, loaded.Count);
        var steam = loaded.Single(i => i.Id == "steam");
        Assert.Equal("games-steam", steam.CategoryId);
        Assert.Equal("Steam", steam.Category); // 兼容字段填充分类名
        var github = loaded.Single(i => i.Id == "github");
        Assert.Null(github.CategoryId);
        Assert.Equal("默认", github.Category);
        Assert.Equal("games-steam", reloaded.Categories[0].Children![0].Id);
    }

    [Fact]
    public void Load_手写新格式JSON_未知字段忽略可选缺省()
    {
        var json = """
        {
          "categories": [
            { "id": "games", "name": "游戏", "children": [] }
          ],
          "items": [
            { "id": "steam", "name": "Steam", "command": "steam", "categoryId": "games", "futureField": "ignored" },
            { "id": "no-name", "command": "notepad" }
          ],
          "extraRoot": true
        }
        """;
        File.WriteAllText(TempFile("launcher.json"), json);

        var loaded = new LauncherStore(TempFile("launcher.json")).Load();

        Assert.Equal(2, loaded.Count);
        Assert.Equal("games", loaded[0].CategoryId);
        Assert.Equal("游戏", loaded[0].Category);
        Assert.Equal("", loaded[1].Name); // 缺省归 "" 而非崩溃
    }

    [Fact]
    public void Load_旧格式数组_自动迁移并写回新格式()
    {
        var legacyJson = """
        [
          { "id": "vs-code", "name": "VS Code", "command": "code", "category": "开发" },
          { "id": "github", "name": "GitHub", "command": "https://github.com", "category": "默认" }
        ]
        """;
        File.WriteAllText(TempFile("launcher.json"), legacyJson);

        var store = new LauncherStore(TempFile("launcher.json"));
        var loaded = store.Load();

        Assert.Equal(2, loaded.Count);
        Assert.Equal("开发", loaded[0].Category);
        Assert.Single(store.Categories);
        Assert.Equal("开发", store.Categories[0].Name);
        Assert.Equal("cat-1", store.Categories[0].Id); // 迁移自动生成唯一 slug
        Assert.Null(loaded[1].CategoryId); // 「默认」→ 未分类
        Assert.Equal("默认", loaded[1].Category);

        // 已写回新格式:再次加载走新结构路径
        var json = File.ReadAllText(TempFile("launcher.json"));
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
        var reloaded = new LauncherStore(TempFile("launcher.json")).Load();
        Assert.Equal(2, reloaded.Count);
    }

    [Fact]
    public void Load_无文件_生成默认数据并落盘新格式()
    {
        var store = new LauncherStore(TempFile("launcher.json"));
        var loaded = store.Load();

        Assert.NotEmpty(loaded);
        Assert.True(File.Exists(TempFile("launcher.json")));
        var json = File.ReadAllText(TempFile("launcher.json"));
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind); // 新结构,非数组
        Assert.NotEmpty(doc.RootElement.GetProperty("categories").EnumerateArray());
    }
}
