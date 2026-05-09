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
        var contentArea = new Border { Padding = new Thickness(12) };

        var initCat = categories.FirstOrDefault(c => c.Id == _initialCategoryId) ?? categories.FirstOrDefault();
        if (initCat != null)
        {
            contentArea.Child = new StackPanel().Vertical().Children(
                new Label
                {
                    Text = initCat.Title,
                    FontSize = 16,
                    FontWeight = FontWeight.SemiBold,
                    Margin = new Thickness(0, 0, 0, 12),
                },
                initCat.CreateView()
            );
        }
        else
            contentArea.Child = new Label { Text = "No settings categories registered." };

        return contentArea;
    }
}
