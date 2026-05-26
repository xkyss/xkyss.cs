namespace MewPad.Core.Services.Impl;

using System.Diagnostics;
using System.Text.Json;
using MewPad.Core.Services;

internal class ConfigurationService : IConfigurationService
{
    private readonly ILogger _logger = new DebugLogger(nameof(ConfigurationService));
    private readonly string _configPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MewPad", "appsettings.json");
    private Dictionary<string, JsonElement> _config = [];

    public ConfigurationService() => LoadConfiguration();

    private void LoadConfiguration()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                using var doc = JsonDocument.Parse(json);
                _config = doc.RootElement.EnumerateObject()
                    .ToDictionary(p => p.Name, p => p.Value.Clone());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Load failed: {ex.Message}", ex);
        }
    }

    public T? GetConfig<T>(string key, T? defaultValue = default)
    {
        if (!_config.TryGetValue(key, out var element)) return defaultValue;
        try { return JsonSerializer.Deserialize<T>(element.GetRawText()); }
        catch (Exception ex)
        {
            _logger.LogWarning($"Deserialization failed for key '{key}': {ex.Message}");
            return defaultValue;
        }
    }

    public void SetConfig(string key, object value)
    {
        var json = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(json);
        _config[key] = doc.RootElement.Clone();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_config, options);
            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Save failed: {ex.Message}", ex);
        }
    }
}