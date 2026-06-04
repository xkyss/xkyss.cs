namespace Mewoo.Controls.Sidebar;

public sealed record SidebarControlOptions
{
    public Action<Exception>? OnActionError { get; init; }
}

