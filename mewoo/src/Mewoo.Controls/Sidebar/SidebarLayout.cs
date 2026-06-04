using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace Mewoo.Controls.Sidebar;

public sealed class SidebarLayout
{
    private Element? _header;
    private Element? _search;
    private Element? _body;

    private SidebarLayout()
    {
    }

    public static SidebarLayout Create() => new();

    public SidebarLayout Header(SidebarHeader header)
    {
        _header = header.Build();
        return this;
    }

    public SidebarLayout Header(Element header)
    {
        _header = header;
        return this;
    }

    public SidebarLayout Search(Element search)
    {
        _search = search;
        return this;
    }

    public SidebarLayout Body(Element body)
    {
        _body = body;
        return this;
    }

    public StackPanel Build()
    {
        var layout = new StackPanel { Orientation = Orientation.Vertical };
        if (_header is not null)
        {
            layout.Add(_header);
        }

        if (_search is not null)
        {
            layout.Add(_search);
        }

        if (_body is not null)
        {
            layout.Add(_body);
        }

        return layout;
    }
}
