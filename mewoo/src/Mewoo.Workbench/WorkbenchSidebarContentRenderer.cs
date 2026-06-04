using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Mewoo.Abstractions.Contributions;

namespace Mewoo.Workbench;

public static class WorkbenchSidebarContentRenderer
{
    public static StackPanel Render(
        ViewContainerDescriptor container,
        Func<SidebarViewDescriptor, Element> createView)
    {
        var views = new StackPanel { Orientation = Orientation.Vertical };
        foreach (var view in container.Views)
        {
            views.Add(createView(view));
        }

        return views;
    }
}

