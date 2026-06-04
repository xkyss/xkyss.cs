namespace Mewoo.Abstractions;

public interface IWorkbenchService
{
    WorkbenchViewState Current { get; }

    event EventHandler<WorkbenchViewStateChangedEventArgs>? StateChanged;

    ValueTask OpenMainViewAsync(string mainViewId, CancellationToken cancellationToken = default);

    void UpdateStatusBarItem(string statusBarItemId, string text);

    void OpenLogsPanel();
}

public sealed record WorkbenchViewState(
    string? ActiveActivityId,
    string? ActiveMainViewId);

public sealed class WorkbenchViewStateChangedEventArgs(
    WorkbenchViewState previous,
    WorkbenchViewState current) : EventArgs
{
    public WorkbenchViewState Previous { get; } = previous;

    public WorkbenchViewState Current { get; } = current;
}
