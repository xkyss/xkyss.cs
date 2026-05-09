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
    private readonly string _initialCategoryId;

    public SettingsContentItem(ShellContext shell, string initialCategoryId = "appearance")
    {
        _shell = shell;
        _initialCategoryId = initialCategoryId;
    }

    public string Id => "mewpad.settings";
    public string Title => "Settings";
    public object? Icon => "⚙";
    public bool CanClose => true;

    public FrameworkElement CreateContent()
    {
        var categories = _shell.Settings.Categories;

        var categoryList = new StackPanel().Vertical();
        var contentArea = new Border { Padding = new Thickness(12) };
        ISettingsCategory? activeCategory = null;

        void SelectCategory(ISettingsCategory cat)
        {
            activeCategory = cat;
            contentArea.Child = cat.CreateView();
            // Rebuild list to update highlight
            RebuildCategoryList();
        }

        void RebuildCategoryList()
        {
            categoryList.Children().Clear();
            foreach (var cat in categories)
            {
                var c = cat;
                var isActive = activeCategory?.Id == c.Id;
                var label = new Label
                {
                    Text = c.Icon != null ? $"{c.Icon}  {c.Title}" : c.Title,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 2),
                };
                var btn = new Button { Content = label, Padding = new Thickness(8, 6) };
                if (isActive)
                    btn.Background = Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF);
                btn.OnClick(() => SelectCategory(c));
                categoryList.Children(btn);
            }
        }

        RebuildCategoryList();

        // Activate initial category
        var initCat = categories.FirstOrDefault(c => c.Id == _initialCategoryId) ?? categories.FirstOrDefault();
        if (initCat != null)
            SelectCategory(initCat);
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
