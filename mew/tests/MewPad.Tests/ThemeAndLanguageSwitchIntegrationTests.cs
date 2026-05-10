using MewPad.Core;
using MewPad.Core.Services;
using MewPad.Core.Services.Impl;
using Xunit;

namespace MewPad.Tests;

/// <summary>
/// 主题与语言切换集成测试
/// </summary>
public class ThemeAndLanguageSwitchIntegrationTests
{
    [Fact]
    public void ThemeSwitch_NotifiesSubscribers()
    {
        // Arrange
        var themeService = new ThemeService();
        var themeChanges = new List<Theme>();

        themeService.Changed.Subscribe(theme => themeChanges.Add(theme));

        // Act
        themeService.Set(Theme.Light);
        themeService.Set(Theme.Dark);
        themeService.Set(Theme.Light);

        // Assert
        Assert.Equal(3, themeChanges.Count);
        Assert.Equal(Theme.Light, themeChanges[0]);
        Assert.Equal(Theme.Dark, themeChanges[1]);
        Assert.Equal(Theme.Light, themeChanges[2]);
    }

    [Fact]
    public void LanguageSwitch_InitializesCorrectly()
    {
        // Arrange & Act
        var localizationService = new LocalizationService();

        // Assert - LocalizationService 应该初始化
        Assert.NotNull(localizationService);
    }

    [Fact]
    public void ThemePersistence_WithConfiguration()
    {
        // Arrange
        var configService1 = new ConfigurationService();
        var themeService1 = new ThemeService();
        var key = "theme.persistence." + Guid.NewGuid();

        // Act
        themeService1.Set(Theme.Light);
        configService1.SetConfig(key, Theme.Light.ToString());
        configService1.Save();

        // Assert - 新实例应能读取
        var configService2 = new ConfigurationService();
        var savedTheme = configService2.GetConfig<string>(key);
        Assert.Equal(Theme.Light.ToString(), savedTheme);
    }

    [Fact]
    public void RuntimeThemeChange_WorksCorrectly()
    {
        // Arrange
        var themeService = new ThemeService();
        var notificationCount = 0;

        // Act
        themeService.Changed.Subscribe(_ => notificationCount++);

        // 模拟运行时切换
        themeService.Toggle();
        themeService.Toggle();
        themeService.Toggle();

        // Assert
        Assert.Equal(3, notificationCount);
        Assert.Equal(Theme.System, themeService.Current); // System -> Light -> Dark -> System
    }

    [Fact]
    public void MultipleServices_ThemeConsistency()
    {
        // Arrange
        var configService = new ConfigurationService();
        var themeService = new ThemeService();

        // Act
        themeService.Set(Theme.Light);
        configService.SetConfig("currentTheme", themeService.Current);

        // Assert - 主题值应该一致
        Assert.Equal(themeService.Current, configService.GetConfig<Theme>("currentTheme"));
    }

    [Fact]
    public void LanguageInitialization_WithDefault()
    {
        // Arrange & Act
        var localizationService = new LocalizationService();

        // Assert
        Assert.NotNull(localizationService);
    }

    [Fact]
    public void ThemeToggle_Cycle()
    {
        // Arrange
        var themeService = new ThemeService();

        // Act & Assert - 验证完整循环
        Assert.Equal(Theme.System, themeService.Current);

        themeService.Toggle();
        Assert.Equal(Theme.Light, themeService.Current);

        themeService.Toggle();
        Assert.Equal(Theme.Dark, themeService.Current);

        themeService.Toggle();
        Assert.Equal(Theme.System, themeService.Current);
    }

    [Fact]
    public void Config_ThemeAndLanguageTogether()
    {
        // Arrange
        var configService = new ConfigurationService();

        // Act
        configService.SetConfig("theme", Theme.Dark);
        configService.SetConfig("language", "zh-CN");
        configService.Save();

        // Assert
        var config2 = new ConfigurationService();
        Assert.Equal(Theme.Dark, config2.GetConfig<Theme>("theme"));
        Assert.Equal("zh-CN", config2.GetConfig<string>("language"));
    }
}
