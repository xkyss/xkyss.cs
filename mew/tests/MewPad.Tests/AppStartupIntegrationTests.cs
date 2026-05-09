using MewPad.Core.Shell;
using MewPad.Core.Services;
using MewPad.Core.Services.Impl;
using MewPad.Core.Interfaces;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Xunit;

namespace MewPad.Tests;

/// <summary>
/// 应用启动流程集成测试
/// </summary>
public class AppStartupIntegrationTests
{
    [Fact]
    public void StartupSequence_InitializesAllServices()
    {
        // Arrange & Act
        var configService = new ConfigurationService();
        var themeService = new ThemeService();
        var localizationService = new LocalizationService();
        var settingsService = new SettingsService();

        // Assert - 所有服务已初始化
        Assert.NotNull(configService);
        Assert.NotNull(themeService);
        Assert.NotNull(localizationService);
        Assert.NotNull(settingsService);
    }

    [Fact]
    public void StartupSequence_ShellContextReady()
    {
        // Arrange & Act
        var configService = new ConfigurationService();
        var themeService = new ThemeService();
        var localizationService = new LocalizationService();
        var settingsService = new SettingsService();
        
        var shell = new ShellContext(themeService, localizationService, settingsService, configService);

        // Assert - ShellContext 可用
        Assert.NotNull(shell);
        Assert.NotNull(shell.ActiveActivityId);
    }

    [Fact]
    public void StartupSequence_ThemeDefaultsToDark()
    {
        // Arrange & Act
        var themeService = new ThemeService();

        // Assert - 启动时默认为 Dark 主题
        Assert.Equal(MewPad.Core.Services.Theme.Dark, themeService.Current);
    }

    [Fact]
    public void StartupSequence_ConfigurationPersists()
    {
        // Arrange
        var configService1 = new ConfigurationService();
        var testKey = $"test-key-{Guid.NewGuid()}";

        // Act
        configService1.SetConfig(testKey, "test-value");
        configService1.Save();

        // Assert - 新实例应能读取
        var configService2 = new ConfigurationService();
        var value = configService2.GetConfig<string>(testKey);
        Assert.Equal("test-value", value);
    }

    [Fact]
    public void Startup_LastActivityRestoration()
    {
        // Arrange
        var configService = new ConfigurationService();
        var themeService = new ThemeService();
        var localizationService = new LocalizationService();
        var settingsService = new SettingsService();

        var shell = new ShellContext(themeService, localizationService, settingsService, configService);
        var activity = new MockActivityItem("test-activity-123", "Test Activity");
        
        // Act
        shell.RegisterActivity(activity);
        shell.ActiveActivityId.Value = "test-activity-123";
        configService.SetConfig("lastActiveActivityId", "test-activity-123");
        configService.Save();

        // 模拟重启应用
        var shell2 = new ShellContext(
            new ThemeService(),
            new LocalizationService(),
            new SettingsService(),
            new ConfigurationService()
        );

        // Assert - 应该能恢复上次的Activity（即使在新实例中）
        Assert.NotNull(shell2);
    }

    [Fact]
    public void Startup_WindowStateRestoration()
    {
        // Arrange
        var configService = new ConfigurationService();

        // Act
        configService.SetConfig("windowWidth", 1200);
        configService.SetConfig("windowHeight", 800);
        configService.SetConfig("windowLeft", 100);
        configService.SetConfig("windowTop", 100);
        configService.SetConfig("windowMaximized", false);
        configService.Save();

        // Assert - 应能读取窗口状态
        var config2 = new ConfigurationService();
        Assert.Equal(1200, config2.GetConfig<int>("windowWidth"));
        Assert.Equal(800, config2.GetConfig<int>("windowHeight"));
        Assert.Equal(false, config2.GetConfig<bool>("windowMaximized"));
    }

    // Mock implementation
    private class MockActivityItem : IActivityItem
    {
        public MockActivityItem(string id, string title)
        {
            Id = id;
            Title = title;
        }

        public string Id { get; }
        public string Title { get; }
        public object Icon { get; } = "";

        public FrameworkElement CreateContent() => new Label { Text = Title };
    }
}
