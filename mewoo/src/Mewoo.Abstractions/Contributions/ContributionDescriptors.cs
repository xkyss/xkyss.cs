using Mewoo.Abstractions.Commands;
using Mewoo.Abstractions.Views;

namespace Mewoo.Abstractions.Contributions;

public sealed record ActivityDescriptor(
    string Id,
    string OwnerPluginId,
    string Title,
    string? Icon,
    string ViewContainerId,
    int Order);

public sealed record ViewContainerDescriptor(
    string Id,
    string OwnerPluginId,
    string Title,
    IReadOnlyList<SidebarViewDescriptor> Views);

public sealed record SidebarViewDescriptor(
    string Id,
    string OwnerPluginId,
    string Title,
    Func<IMewooViewContext, IMewooView> CreateView);

public sealed record MainViewDescriptor(
    string Id,
    string OwnerPluginId,
    string Title,
    bool CanOpenMultiple,
    Func<IMewooViewContext, IMewooView> CreateView);

public enum StatusBarAlignment
{
    Left,
    Right,
}

public sealed record StatusBarItemDescriptor(
    string Id,
    string OwnerPluginId,
    StatusBarAlignment Alignment,
    string Text,
    string? CommandId);

public sealed record ThemeTokenOverrideDescriptor(
    string Id,
    string OwnerPluginId,
    IReadOnlyDictionary<string, object> Tokens);

public sealed record MewooContributionSnapshot(
    IReadOnlyList<ActivityDescriptor> Activities,
    IReadOnlyList<ViewContainerDescriptor> ViewContainers,
    IReadOnlyList<MainViewDescriptor> MainViews,
    IReadOnlyList<MewooCommandDescriptor> Commands,
    IReadOnlyList<StatusBarItemDescriptor> StatusBarItems,
    IReadOnlyList<ThemeTokenOverrideDescriptor> ThemeTokenOverrides);

