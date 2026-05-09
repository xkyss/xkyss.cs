namespace MewPad.Hosting.Extensions;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewPad.Core.Interfaces;

/// <summary>
/// Search activity - shows a search input placeholder in the SideBar.
/// </summary>
public class SearchActivity : IActivityItem
{
    public string Id => "search";
    public object Icon => "🔍";
    public string Title => "Search";
    public int Order => 2;

    public FrameworkElement CreateContent() =>
        new StackPanel().Vertical().Children(
            new Label { Text = "SEARCH" },
            new Label { Text = "Search across files..." }
        );
}
