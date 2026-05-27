namespace Mewoo.Abstractions;

public interface IWorkbenchService
{
    ValueTask OpenMainViewAsync(string mainViewId, CancellationToken cancellationToken = default);

    void UpdateStatusBarItem(string statusBarItemId, string text);
}
