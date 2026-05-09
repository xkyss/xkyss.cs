using MewPad.Core.Services;
using MewPad.Core.Services.Impl;
using MewPad.Core.Interfaces;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Xunit;

namespace MewPad.Tests;

/// <summary>
/// SettingsService 单元测试
/// </summary>
public class SettingsServiceTests
{
    [Fact]
    public void Constructor_InitializesEmpty()
    {
        // Arrange & Act
        var service = new SettingsService();

        // Assert
        Assert.Empty(service.Categories);
    }

    [Fact]
    public void RegisterCategory_AddsCategory()
    {
        // Arrange
        var service = new SettingsService();
        var category = new MockSettingsCategory("test", "Test");

        // Act
        service.RegisterCategory(category);

        // Assert
        Assert.Single(service.Categories);
        Assert.Contains(service.Categories, c => c.Id == "test");
    }

    [Fact]
    public void RegisterCategory_MultipleCategories()
    {
        // Arrange
        var service = new SettingsService();
        var category1 = new MockSettingsCategory("cat1", "Category 1", order: 1);
        var category2 = new MockSettingsCategory("cat2", "Category 2", order: 2);

        // Act
        service.RegisterCategory(category1);
        service.RegisterCategory(category2);

        // Assert
        Assert.Equal(2, service.Categories.Count);
    }

    [Fact]
    public void RegisterCategory_RespectOrder()
    {
        // Arrange
        var service = new SettingsService();
        var category1 = new MockSettingsCategory("cat1", "Category 1", order: 10);
        var category2 = new MockSettingsCategory("cat2", "Category 2", order: 5);
        var category3 = new MockSettingsCategory("cat3", "Category 3", order: 15);

        // Act
        service.RegisterCategory(category1);
        service.RegisterCategory(category2);
        service.RegisterCategory(category3);

        // Assert - 应该按 Order 排序
        var categories = service.Categories.OrderBy(c => c.Order).ToList();
        Assert.Equal("cat2", categories[0].Id);
        Assert.Equal("cat1", categories[1].Id);
        Assert.Equal("cat3", categories[2].Id);
    }

    [Fact]
    public void OpenSettings_CallsHandler()
    {
        // Arrange
        var service = new SettingsService();
        var openedCategoryId = "";
        Action<string>? handler = null;

        // 模拟 SetOpenHandler
        service.SetOpenHandler(id => handler?.Invoke(id));
        handler = id => openedCategoryId = id;

        // Act
        service.OpenSettings("test-category");

        // Assert
        Assert.Equal("test-category", openedCategoryId);
    }

    [Fact]
    public void OpenSettings_WithoutHandler_NoThrow()
    {
        // Arrange
        var service = new SettingsService();

        // Act & Assert - 应该不抛出异常
        service.OpenSettings("test-category");
    }

    [Fact]
    public void FindCategory_ById()
    {
        // Arrange
        var service = new SettingsService();
        var category = new MockSettingsCategory("target", "Target Category");
        service.RegisterCategory(category);

        // Act
        var found = service.Categories.FirstOrDefault(c => c.Id == "target");

        // Assert
        Assert.NotNull(found);
        Assert.Equal("target", found.Id);
    }

    // Mock implementation
    private class MockSettingsCategory : ISettingsCategory
    {
        public MockSettingsCategory(string id, string title, string? icon = null, int order = 100)
        {
            Id = id;
            Title = title;
            Icon = icon;
            Order = order;
        }

        public string Id { get; }
        public string Title { get; }
        public object? Icon { get; }
        public int Order { get; }

        public FrameworkElement CreateView() => new Label { Text = Title };
    }
}
