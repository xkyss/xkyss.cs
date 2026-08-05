using System.IO;
using Mew.Launcher;
using Xunit;

namespace Mew.Launcher.Tests;

/// <summary>
/// 应用设置(settings.json)读写:形态偏好等字段往返。
/// </summary>
public class SettingsStoreTests : IDisposable
{
    private readonly string _dir;

    public SettingsStoreTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "mew-settings-tests-" + Guid.NewGuid().ToString("N"));
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
    public void SaveThenLoad_形态偏好与主题往返()
    {
        var store = new SettingsStore(TempFile("settings.json"));
        store.Save(new AppSettings { ItemsViewMode = "list", ThemeMode = "Dark", OverlayHotkey = "Ctrl+Shift+Space" });

        var loaded = new SettingsStore(TempFile("settings.json")).Load();

        Assert.Equal("list", loaded.ItemsViewMode);
        Assert.Equal("Dark", loaded.ThemeMode);
        Assert.Equal("Ctrl+Shift+Space", loaded.OverlayHotkey);
    }

    [Fact]
    public void Load_无文件_返回缺省()
    {
        var loaded = new SettingsStore(TempFile("settings.json")).Load();

        Assert.Null(loaded.ItemsViewMode);
        Assert.Null(loaded.ThemeMode);
    }

    [Fact]
    public void Load_手写JSON_未知字段忽略()
    {
        File.WriteAllText(TempFile("settings.json"), """{ "itemsViewMode": "card", "future": true }""");

        var loaded = new SettingsStore(TempFile("settings.json")).Load();

        Assert.Equal("card", loaded.ItemsViewMode);
    }
}
