using MewPad.Core;
using MewPad.Core.Shell;
using MewPad.Core.Interfaces;
using MewPad.Core.Services;
using MewPad.Core.Services.Impl;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Xunit;

namespace MewPad.Tests;

/// <summary>
/// ShellContext 单元测试
/// </summary>
public class ShellContextTests
{
    [Fact]
    public void Constructor_InitializesDefaultState()
    {
        // Arrange & Act
        var shell = new ShellContext();

        // Assert
        Assert.NotNull(shell.Theme);
        Assert.NotNull(shell.Localization);
        Assert.NotNull(shell.Configuration);
        Assert.NotNull(shell.Settings);
    }

    [Fact]
    public void ActiveActivityId_CanBeSet()
    {
        // Arrange
        var shell = new ShellContext();
        var expectedId = "test-activity";

        // Act
        shell.ActiveActivityId.Value = expectedId;

        // Assert
        Assert.Equal(expectedId, shell.ActiveActivityId.Value);
    }

    [Fact]
    public void ActiveActivityId_NotifiesOnChange()
    {
        // Arrange
        var shell = new ShellContext();
        var changeCount = 0;
        shell.ActiveActivityId.Changed.Subscribe(_ => changeCount++);

        // Act
        shell.ActiveActivityId.Value = "activity1";
        shell.ActiveActivityId.Value = "activity2";

        // Assert
        Assert.Equal(2, changeCount);
    }

    [Fact]
    public void SideBarCollapsed_ToggleBehavior()
    {
        // Arrange
        var shell = new ShellContext();

        // Act & Assert
        Assert.False(shell.SideBarCollapsed.Value);
        shell.SideBarCollapsed.Value = true;
        Assert.True(shell.SideBarCollapsed.Value);
    }

    [Fact]
    public void PanelCollapsed_ToggleBehavior()
    {
        // Arrange
        var shell = new ShellContext();

        // Act & Assert
        Assert.False(shell.PanelCollapsed.Value);
        shell.PanelCollapsed.Value = true;
        Assert.True(shell.PanelCollapsed.Value);
    }

    [Fact]
    public void GetActivities_ReturnsAllRegistered()
    {
        // Arrange
        var shell = new ShellContext();
        var activity1 = new MockActivityItem("act1", "Activity 1");
        var activity2 = new MockActivityItem("act2", "Activity 2");

        // Act
        shell.RegisterActivity(activity1);
        shell.RegisterActivity(activity2);

        // Assert
        var activities = shell.GetActivities();
        Assert.Equal(2, activities.Count);
        Assert.Contains(activities, a => a.Id == "act1");
        Assert.Contains(activities, a => a.Id == "act2");
    }

    [Fact]
    public void RegisterAndGetActivity()
    {
        // Arrange
        var shell = new ShellContext();
        var activity = new MockActivityItem("test-id", "Test Activity");

        // Act
        shell.RegisterActivity(activity);
        var retrieved = shell.GetActivity("test-id");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("test-id", retrieved.Id);
        Assert.Equal("Test Activity", retrieved.Title);
    }

    [Fact]
    public void GetActivity_ReturnsNullIfNotFound()
    {
        // Arrange
        var shell = new ShellContext();

        // Act
        var activity = shell.GetActivity("non-existent");

        // Assert
        Assert.Null(activity);
    }

    [Fact]
    public void Theme_InitializedWithDefault()
    {
        // Arrange & Act
        var shell = new ShellContext();

        // Assert
        Assert.NotNull(shell.Theme);
        Assert.NotNull(shell.Theme.Current);
    }

    [Fact]
    public void Localization_InitializedWithDefault()
    {
        // Arrange & Act
        var shell = new ShellContext();

        // Assert
        Assert.NotNull(shell.Localization);
        Assert.NotNull(shell.Localization.CurrentLanguage);
    }

    // Mock implementation for testing
    private class MockActivityItem : IActivityItem
    {
        public MockActivityItem(string id, string title)
        {
            Id = id;
            Title = title;
        }

        public string Id { get; }
        public object Icon => "📋";
        public string Title { get; }
        public int Order => 100;

        public FrameworkElement CreateContent() =>
            new Label { Text = Title };
    }
}
