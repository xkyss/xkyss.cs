using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace Mewoo.Controls.Sidebar;

public static class SidebarSearchRow
{
    public static TextBox Create(
        string placeholder,
        Action<string>? onTextChanged = null,
        string text = "")
    {
        var search = new TextBox()
            .Text(text)
            .Placeholder(placeholder)
            .Margin(12, 0, 12, 8);
        search.Height = 30;
        search.Padding = new Thickness(8, 4);

        if (onTextChanged is not null)
        {
            search.OnTextChanged(onTextChanged);
        }

        return search;
    }
}

