using Mewoo.Abstractions.Contributions;

namespace Mewoo.Workbench;

public sealed class WorkbenchStatusBarModel
{
    private readonly Dictionary<string, string> _textById = new(StringComparer.Ordinal);

    public void UpdateText(string statusBarItemId, string text)
    {
        _textById[statusBarItemId] = text;
    }

    public void RemoveMissing(IReadOnlySet<string> availableStatusBarItemIds)
    {
        foreach (var statusBarItemId in _textById.Keys.ToArray())
        {
            if (!availableStatusBarItemIds.Contains(statusBarItemId))
            {
                _textById.Remove(statusBarItemId);
            }
        }
    }

    public IReadOnlyList<WorkbenchStatusBarItemRenderModel> GetVisibleItems(
        IEnumerable<StatusBarItemDescriptor> items,
        string? activeActivityId)
    {
        return items
            .Where(item => IsVisible(item, activeActivityId))
            .Select(item => new WorkbenchStatusBarItemRenderModel(
                item,
                _textById.TryGetValue(item.Id, out var text) ? text : item.Text))
            .ToArray();
    }

    public static bool IsVisible(StatusBarItemDescriptor item, string? activeActivityId)
    {
        return item.IsGlobal
            || (activeActivityId is not null
                && string.Equals(item.ActivityScopeId, activeActivityId, StringComparison.Ordinal));
    }
}

public sealed record WorkbenchStatusBarItemRenderModel(StatusBarItemDescriptor Descriptor, string Text);
