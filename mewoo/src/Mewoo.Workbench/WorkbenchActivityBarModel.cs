using Mewoo.Abstractions.Contributions;

namespace Mewoo.Workbench;

public static class WorkbenchActivityBarModel
{
    public static WorkbenchActivityBarSections CreateSections(IEnumerable<ActivityDescriptor> activities)
    {
        var ordered = activities
            .OrderBy(activity => activity.Order)
            .ThenBy(activity => activity.Id)
            .ToArray();

        return new WorkbenchActivityBarSections(
            ordered.Where(activity => activity.Section == ActivityBarSection.Primary).ToArray(),
            ordered.Where(activity => activity.Section == ActivityBarSection.System).ToArray());
    }
}

public sealed record WorkbenchActivityBarSections(
    IReadOnlyList<ActivityDescriptor> Primary,
    IReadOnlyList<ActivityDescriptor> System);
