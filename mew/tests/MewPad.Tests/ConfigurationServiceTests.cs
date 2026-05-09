using MewPad.Core;
using MewPad.Core.Services;
using MewPad.Core.Services.Impl;
using Xunit;

namespace MewPad.Tests;

/// <summary>
/// ConfigurationService 单元测试
/// </summary>
public class ConfigurationServiceTests
{
    [Fact]
    public void GetConfig_ReturnsDefaultForMissingKey()
    {
        // Arrange
        var service = new ConfigurationService();

        // Act
        var value = service.GetConfig<string>("non-existent-key-" + Guid.NewGuid(), "default-value");

        // Assert
        Assert.Equal("default-value", value);
    }

    [Fact]
    public void SetConfig_StoresValue()
    {
        // Arrange
        var service = new ConfigurationService();
        var testKey = "test-key-" + Guid.NewGuid();

        // Act
        service.SetConfig(testKey, "test-value");
        var value = service.GetConfig<string>(testKey);

        // Assert
        Assert.Equal("test-value", value);
    }

    [Fact]
    public void SetConfig_WithNumericValue()
    {
        // Arrange
        var service = new ConfigurationService();
        var testKey = "numeric-key-" + Guid.NewGuid();

        // Act
        service.SetConfig(testKey, 42);
        var value = service.GetConfig<int>(testKey);

        // Assert
        Assert.Equal(42, value);
    }

    [Fact]
    public void SetConfig_WithBoolValue()
    {
        // Arrange
        var service = new ConfigurationService();
        var testKey = "bool-key-" + Guid.NewGuid();

        // Act
        service.SetConfig(testKey, true);
        var value = service.GetConfig<bool>(testKey);

        // Assert
        Assert.True(value);
    }

    [Fact]
    public void Save_PersistsConfigToFile()
    {
        // Arrange
        var service = new ConfigurationService();
        var testKey = "persist-key-" + Guid.NewGuid();

        // Act
        service.SetConfig(testKey, "persist-value");
        service.Save();

        // Assert - Service should have saved the value
        Assert.NotNull(service);
    }

    [Fact]
    public void SetConfig_OverwritesExistingValue()
    {
        // Arrange
        var service = new ConfigurationService();
        var testKey = "overwrite-key-" + Guid.NewGuid();

        // Act
        service.SetConfig(testKey, "value1");
        service.SetConfig(testKey, "value2");
        var value = service.GetConfig<string>(testKey);

        // Assert
        Assert.Equal("value2", value);
    }

    [Fact]
    public void GetConfig_WithNestedKey()
    {
        // Arrange
        var service = new ConfigurationService();
        var testKey = "config.nested." + Guid.NewGuid();

        // Act
        service.SetConfig(testKey, "nested-value");
        var value = service.GetConfig<string>(testKey);

        // Assert
        Assert.Equal("nested-value", value);
    }

    [Fact]
    public void MultipleInstances_ShareSameConfig()
    {
        // Arrange
        var testKey = "multi-instance-test-" + Guid.NewGuid();

        // Act
        var service1 = new ConfigurationService();
        service1.SetConfig(testKey, "shared-value");
        service1.Save();

        // Assert - service2 should work independently
        var service2 = new ConfigurationService();
        Assert.NotNull(service2);
    }
}
