namespace MewPad.Hosting.Extensions;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewPad.Core.Interfaces;

/// <summary>
/// Explorer activity - shows a file tree placeholder in the SideBar.
/// </summary>
public class ExplorerActivity : IActivityItem
{
    public string Id => "explorer";
    public object Icon => "📁";
    public string Title => "Explorer";
    public int Order => 1;

    public FrameworkElement CreateContent() =>
        new StackPanel().Vertical().Children(
            new Label { Text = "EXPLORER" },
            new Label { Text = "(no workspace open)" },
            new Button().Content("Open Folder...")
        );
}
