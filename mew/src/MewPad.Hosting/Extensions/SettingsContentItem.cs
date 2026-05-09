namespace MewPad.Hosting.Extensions;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewPad.Core.Interfaces;
using MewPad.Core.Shell;

/// <summary>
/// Built-in Settings panel, opened as a content tab (id: mewpad.settings).
/// </summary>
public sealed class SettingsContentItem : IContentItem
{
    private readonly ShellContext _shell;

    public SettingsContentItem(ShellContext shell) => _shell = shell;

    public string Id => "mewpad.settings";
    public string Title => "Settings";
    public object? Icon => "⚙";
    public bool CanClose => true;

    public FrameworkElement CreateContent()
    {
        var categories = _shell.Settings.Categories;

        var categoryList = new StackPanel().Vertical();
        var contentArea = new Border { Padding = new Thickness(12) };

        void SelectCategory(ISettingsCategory cat)
        {
            contentArea.Child = cat.CreateView();
        }

        foreach (var cat in categories)
        {
            var c = cat;
            categoryList.Children(
                new Button()
                    .Content(new Label
                    {
                        Text = c.Icon != null ? $"{c.Icon}  {c.Title}" : c.Title,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    })
                    .OnClick(() => SelectCategory(c)));
        }

        if (categories.Count > 0)
            SelectCategory(categories[0]);
        else
            contentArea.Child = new Label { Text = "No settings categories registered." };

        var categoryScroll = new Border
        {
            Padding = new Thickness(4),
            Child = categoryList,
        };

        return new SplitPanel
        {
            Orientation = Orientation.Horizontal,
            FirstLength = GridLength.Pixels(200),
            SecondLength = GridLength.Stars(1),
            MinFirst = 150,
            MinSecond = 200,
            First = categoryScroll,
            Second = contentArea,
        };
    }
}
