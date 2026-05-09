namespace MewPad.Core.Services;

/// <summary>
/// Service for application configuration persistence.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Get a configuration value.
    /// </summary>
    T? GetConfig<T>(string key, T? defaultValue = default);

    /// <summary>
    /// Set a configuration value in memory.
    /// </summary>
    void SetConfig(string key, object value);

    /// <summary>
    /// Save all configuration changes to disk.
    /// </summary>
    void Save();
}