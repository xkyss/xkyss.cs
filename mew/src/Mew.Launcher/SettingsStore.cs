using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mew.Launcher;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true)]
[JsonSerializable(typeof(AppSettings))]
internal sealed partial class AppSettingsJsonContext : JsonSerializerContext;

/// <summary>
/// 应用设置(主题模式、呼出热键)。
/// 读写使用源生成序列化,避免运行时反射(AOT/Trim 兼容)。
/// </summary>
internal sealed class AppSettings
{
    /// <summary>主题模式,取值 ThemeVariant 枚举名:System/Light/Dark,缺省 System。</summary>
    public string? ThemeMode { get; set; }

    /// <summary>浮层呼出热键,形如 Ctrl+Alt+Space,缺省 Ctrl+Alt+Space。</summary>
    public string? OverlayHotkey { get; set; }
}

/// <summary>
/// 设置数据源:%APPDATA%\Mew\settings.json,启动时加载、变更时保存;
/// 与启动项数据(launcher.json)职责分离。
/// </summary>
internal sealed class SettingsStore
{
    public SettingsStore()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        FilePath = Path.Combine(appData, "Mew", "settings.json");
    }

    public string FilePath { get; }

    public AppSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize(json, AppSettingsJsonContext.Default.AppSettings) ?? new AppSettings();
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
        }

        return new AppSettings();
    }

    public void Save(AppSettings settings)
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
            JsonSerializer.Serialize(writer, settings, AppSettingsJsonContext.Default.AppSettings);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
