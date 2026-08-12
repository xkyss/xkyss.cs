using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace Mew.Workbench;

/// <summary>
/// 分节设置服务:%APPDATA%\Mew\settings.json。根节为宿主级设置(主题模式、浮层呼出键),
/// 工具模块设置按模块 Id 分节(如 launcher 节的列表形态);旧扁平结构首次加载自动迁移。
/// 内部用 JsonObject DOM(无反射,AOT/Trim 兼容),模块节经调用方源生成类型往返。
/// </summary>
public sealed class SettingsService : ISettingsService
{
    private JsonObject _root = [];

    public SettingsService(string? filePath = null)
    {
        FilePath = filePath
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mew", "settings.json");
    }

    public string FilePath { get; }

    /// <summary>宿主级设置:主题模式,取值 ThemeVariant 枚举名:System/Light/Dark。</summary>
    public string? ThemeMode { get => GetString("themeMode"); set => SetString("themeMode", value); }

    /// <summary>宿主级设置:浮层呼出热键,形如 Ctrl+Alt+Space。</summary>
    public string? OverlayHotkey { get => GetString("overlayHotkey"); set => SetString("overlayHotkey", value); }

    /// <summary>读取工具模块设置节:无该节或节类型不匹配时返回 null。</summary>
    public T? ReadSection<T>(string moduleId, JsonTypeInfo<T> typeInfo) where T : class
        => _root[moduleId] is JsonObject section ? JsonSerializer.Deserialize(section, typeInfo) : null;

    /// <summary>写入工具模块设置节(覆盖该模块整节)。</summary>
    public void WriteSection<T>(string moduleId, T value, JsonTypeInfo<T> typeInfo)
        => _root[moduleId] = JsonSerializer.SerializeToNode(value, typeInfo);

    /// <summary>从磁盘加载设置;旧扁平结构(itemsViewMode 在根节)自动迁移到 launcher 模块节并回写。</summary>
    public void Load()
    {
        try
        {
            _root = File.Exists(FilePath)
                ? JsonNode.Parse(File.ReadAllText(FilePath)) as JsonObject ?? []
                : [];
            MigrateLegacyFlatSettings();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            _root = [];
        }
    }

    /// <summary>写回磁盘;目录缺失时创建,保留缩进与中文可读。</summary>
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, _root.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            }));
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    /// <summary>旧扁平结构迁移:根节的 itemsViewMode(旧 Launcher 形态偏好)移入 launcher 模块节。</summary>
    private void MigrateLegacyFlatSettings()
    {
        if (_root.TryGetPropertyValue("itemsViewMode", out var legacy) && legacy is not null)
        {
            _root.Remove("itemsViewMode");
            var section = _root["launcher"] as JsonObject ?? [];
            _root["launcher"] = section;
            section["itemsViewMode"] = legacy.DeepClone();
            Save();
        }
    }

    private string? GetString(string key) => _root[key]?.GetValue<string>();

    private void SetString(string key, string? value)
    {
        if (value is null)
        {
            _root.Remove(key);
        }
        else
        {
            _root[key] = value;
        }
    }
}
