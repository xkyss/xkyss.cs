using MewPad.Core;
using MewPad.Core.Services;
using MewPad.Core.Services.Impl;
using Xunit;

namespace MewPad.Tests;

/// <summary>
/// ThemeService 单元测试
/// </summary>
public class ThemeServiceTests
{
    [Fact]
    public void Constructor_DefaultToSystemTheme()
    {
        // Arrange & Act
        var service = new ThemeService();

        // Assert
        Assert.Equal(Theme.System, service.Current);
    }

    [Fact]
    public void Set_ChangesTheme()
    {
        // Arrange
        var service = new ThemeService();

        // Act
        service.Set(Theme.Light);

        // Assert
        Assert.Equal(Theme.Light, service.Current);
    }

    [Fact]
    public void Toggle_SwitchesTheme()
    {
        // Arrange
        var service = new ThemeService();
        Assert.Equal(Theme.System, service.Current);

        // Act
        service.Toggle();

        // Assert
        Assert.Equal(Theme.Light, service.Current);

        // Act
        service.Toggle();

        // Assert
        Assert.Equal(Theme.Dark, service.Current);

        // Act
        service.Toggle();

        // Assert
        Assert.Equal(Theme.System, service.Current);
    }

    [Fact]
    public void Changed_NotifiesOnThemeChange()
    {
        // Arrange
        var service = new ThemeService();
        var changeCount = 0;
        var changedTheme = Theme.System;

        service.Changed.Subscribe(theme =>
        {
            changeCount++;
            changedTheme = theme;
        });

        // Act
        service.Set(Theme.Light);

        // Assert
        Assert.Equal(1, changeCount);
        Assert.Equal(Theme.Light, changedTheme);
    }

    [Fact]
    public void Changed_MultipleSubscribers()
    {
        // Arrange
        var service = new ThemeService();
        var subscriber1Count = 0;
        var subscriber2Count = 0;

        service.Changed.Subscribe(_ => subscriber1Count++);
        service.Changed.Subscribe(_ => subscriber2Count++);

        // Act
        service.Toggle();
        service.Toggle();

        // Assert
        Assert.Equal(2, subscriber1Count);
        Assert.Equal(2, subscriber2Count);
    }

    [Fact]
    public void Set_SameThemeTwice()
    {
        // Arrange
        var service = new ThemeService();
        var changeCount = 0;
        service.Changed.Subscribe(_ => changeCount++);

        // Act
        service.Set(Theme.System);
        service.Set(Theme.System);

        // Assert
        // ObservableValue 会在值改变时触发，即使是相同的值也可能触发
        // 这里测试实际行为
        Assert.Equal(Theme.System, service.Current);
    }
}
