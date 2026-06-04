using Aprillz.MewUI.Controls;
using Mewoo.Controls.Sidebar;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class SidebarHeaderAndSearchControlsTests
{
    [TestMethod]
    public void SidebarHeaderBuildsThreePartHeader()
    {
        var header = SidebarHeader.Create("Plugins")
            .Action("+", "Install", () => { })
            .Action("R", "Reload", () => { })
            .More(menu => menu.Item("Open diagnostics", () => { }))
            .Build();

        Assert.IsInstanceOfType<Grid>(header);
        var grid = (Grid)header;
        Assert.AreEqual(3, grid.Count);
        CollectionAssert.AreEqual(
            new[] { 0, 1, 2 },
            grid.Children.Select(Grid.GetColumn).ToArray());

        var actions = (StackPanel)grid[1];
        Assert.AreEqual(2, actions.Count);
        for (var index = 0; index < actions.Count; index++)
        {
            var button = (Button)actions[index];
            Assert.AreEqual(0, button.BorderThickness);
        }

        var right = (StackPanel)grid[2];
        Assert.AreEqual(1, right.Count);
        var more = (MenuBar)right[0];
        Assert.AreEqual(0, more.BorderThickness);
        Assert.IsFalse(more.DrawBottomSeparator);
        Assert.AreEqual(1, more.Items.Count);
    }

    [TestMethod]
    public void SidebarLayoutCanComposeHeaderSearchAndBody()
    {
        var layout = SidebarLayout.Create()
            .Header(SidebarHeader.Create("Launcher"))
            .Search(SidebarSearchRow.Create("Search shortcuts"))
            .Body(new StackPanel())
            .Build();

        Assert.AreEqual(3, layout.Count);
        Assert.IsInstanceOfType<Grid>(layout[0]);
        Assert.IsInstanceOfType<TextBox>(layout[1]);
        Assert.IsInstanceOfType<StackPanel>(layout[2]);
    }
}
