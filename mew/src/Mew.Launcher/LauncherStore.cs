using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mew.Launcher;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true)]
[JsonSerializable(typeof(List<LauncherItem>))]
internal sealed partial class LauncherJsonContext : JsonSerializerContext;

/// <summary>
/// 启动项数据源:单一 JSON 配置文件,首次运行生成默认示例,变更后由调用方保存。
/// 读写使用源生成序列化,避免运行时反射(AOT/Trim 兼容)。
/// </summary>
internal sealed class LauncherStore
{
    public LauncherStore()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        FilePath = Path.Combine(appData, "Mew", "launcher.json");
    }

    public string FilePath { get; }

    public List<LauncherItem> Load()
    {
        if (!File.Exists(FilePath))
        {
            var defaults = DefaultItems();
            Save(defaults);
            return defaults;
        }

        try
        {
            var json = File.ReadAllText(FilePath);
            var list = JsonSerializer.Deserialize(json, LauncherJsonContext.Default.ListLauncherItem) ?? [];
            return Normalize(list);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return DefaultItems();
        }
    }

    public void Save(List<LauncherItem> items)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

            // 缩进 + 保留中文可读,便于手写与 diff;序列化元数据仍走源生成
            var options = new JsonWriterOptions
            {
                Indented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            using var stream = File.Create(FilePath);
            using var writer = new Utf8JsonWriter(stream, options);
            JsonSerializer.Serialize(writer, items, LauncherJsonContext.Default.ListLauncherItem);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    /// <summary>
    /// 手写 JSON 可能省略可选字段(STJ 置 null),在此归一并消除下游空引用风险。
    /// </summary>
    private static List<LauncherItem> Normalize(List<LauncherItem> items) =>
        items.Select(item => item with
        {
            Name = item.Name ?? "",
            Command = item.Command ?? "",
            Category = item.Category ?? "默认",
        }).ToList();

    private static List<LauncherItem> DefaultItems() =>
    [
        new LauncherItem("vs-code", "VS Code", "code", Category: "开发"),
        new LauncherItem("terminal", "Terminal", "wt", Category: "开发"),
        new LauncherItem("github", "GitHub", "https://github.com", Category: "工具"),
        new LauncherItem("notepad", "记事本", "notepad", Category: "工具"),
    ];
}
