using Aprillz.MewUI.Controls;
using Mewoo.Abstractions.Contributions;
using Mewoo.Abstractions.Views;
using Mewoo.Workbench;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class WorkbenchSidebarContentRendererTests
{
    [TestMethod]
    public void RenderHostsSidebarViewsWithoutMetadataTitleChrome()
    {
        var container = new ViewContainerDescriptor(
            "plugin.views",
            "plugin",
            "plugin.activity",
            "Metadata Container Title",
            [
                SidebarView("first.sidebar", "First Metadata Title"),
                SidebarView("second.sidebar", "Second Metadata Title"),
            ]);

        var rendered = WorkbenchSidebarContentRenderer.Render(
            container,
            view => new Border { Tag = view.Title });

        Assert.AreEqual(2, rendered.Count);
        Assert.IsInstanceOfType<Border>(rendered[0]);
        Assert.IsInstanceOfType<Border>(rendered[1]);
        CollectionAssert.AreEqual(
            new[] { "First Metadata Title", "Second Metadata Title" },
            rendered.Children.Cast<Border>().Select(border => border.Tag).ToArray());
    }

    private static SidebarViewDescriptor SidebarView(string id, string title)
    {
        return new SidebarViewDescriptor(
            id,
            "plugin",
            "plugin.activity",
            title,
            _ => new MewooView(id, new object()));
    }
}

