using MewPad.Core.Services;
using MewPad.Core.Services.Impl;
using Xunit;
using System.IO;
using System.Text.Json;

namespace MewPad.Tests;

/// <summary>
/// ConfigurationService 单元测试
/// </summary>
public class ConfigurationServiceTests
{
    private readonly string _testConfigPath;

    public ConfigurationServiceTests()
    {
        // 使用临时目录
        _testConfigPath = Path.Combine(Path.GetTempPath(), $"mewpad-test-{Guid.NewGuid()}.json");
    }

    public void Dispose()
    {
        // 清理临时文件
        if (File.Exists(_testConfigPath))
            File.Delete(_testConfigPath);
    }

    [Fact]
    public void GetConfig_ReturnsDefaultForMissingKey()
    {
        // Arrange
        var service = new ConfigurationService(_testConfigPath);

        // Act
        var value = service.GetConfig("non.existent.key", "default-value");

        // Assert
        Assert.Equal("default-value", value);
    }

    [Fact]
    public void SetConfig_StoresValue()
    {
        // Arrange
        var service = new ConfigurationService(_testConfigPath);

        // Act
        service.SetConfig("test.key", "test-value");

        // Assert
        var value = service.GetConfig<string>("test.key");
        Assert.Equal("test-value", value);
    }

    [Fact]
    public void SetConfig_WithNumericValue()
    {
        // Arrange
        var service = new ConfigurationService(_testConfigPath);

        // Act
        service.SetConfig("numeric.key", 42);

        // Assert
        var value = service.GetConfig("numeric.key", 0);
        Assert.Equal(42, value);
    }

    [Fact]
    public void SetConfig_WithBoolValue()
    {
        // Arrange
        var service = new ConfigurationService(_testConfigPath);

        // Act
        service.SetConfig("bool.key", true);

        // Assert
        var value = service.GetConfig("bool.key", false);
        Assert.True(value);
    }

    [Fact]
    public void Save_PersistsConfigToFile()
    {
        // Arrange
        var service = new ConfigurationService(_testConfigPath);
        service.SetConfig("persistent.key", "persistent-value");

        // Act
        service.Save();

        // Assert
        Assert.True(File.Exists(_testConfigPath));
        var json = File.ReadAllText(_testConfigPath);
        Assert.NotEmpty(json);
    }

    [Fact]
    public void LoadFromFile_RestoresPersistedConfig()
    {
        // Arrange
        var service1 = new ConfigurationService(_testConfigPath);
        service1.SetConfig("persistent.key", "persistent-value");
        service1.Save();

        // Act - 创建新的服务实例，应该加载持久化的配置
        var service2 = new ConfigurationService(_testConfigPath);
        var value = service2.GetConfig<string>("persistent.key");

        // Assert
        Assert.Equal("persistent-value", value);
    }

    [Fact]
    public void SetConfig_OverwritesExistingValue()
    {
        // Arrange
        var service = new ConfigurationService(_testConfigPath);
        service.SetConfig("key", "value1");

        // Act
        service.SetConfig("key", "value2");

        // Assert
        var value = service.GetConfig<string>("key");
        Assert.Equal("value2", value);
    }

    [Fact]
    public void GetConfig_WithNestedKey()
    {
        // Arrange
        var service = new ConfigurationService(_testConfigPath);

        // Act
        service.SetConfig("ui.theme", "Dark");
        service.SetConfig("ui.language", "zh-CN");

        // Assert
        Assert.Equal("Dark", service.GetConfig<string>("ui.theme"));
        Assert.Equal("zh-CN", service.GetConfig<string>("ui.language"));
    }

    [Fact]
    public void MultipleInstances_ShareSameConfig()
    {
        // Arrange
        var service1 = new ConfigurationService(_testConfigPath);
        service1.SetConfig("shared.key", "value1");
        service1.Save();

        // Act
        var service2 = new ConfigurationService(_testConfigPath);
        var value = service2.GetConfig<string>("shared.key");

        // Assert
        Assert.Equal("value1", value);
    }
}
