using Aprillz.MewUI.Controls;

namespace Mewoo.Controls.Sidebar;

internal static class SidebarActionRunner
{
    public static Action ForButton(
        Button button,
        Func<CancellationToken, ValueTask> action,
        SidebarControlOptions? options)
    {
        return async () =>
        {
            if (!button.IsEnabled)
            {
                return;
            }

            button.IsEnabled = false;
            try
            {
                await action(CancellationToken.None);
            }
            catch (Exception exception)
            {
                options?.OnActionError?.Invoke(exception);
                if (options?.OnActionError is null)
                {
                    throw;
                }
            }
            finally
            {
                button.IsEnabled = true;
            }
        };
    }

    public static Action ForMenuItem(
        Func<CancellationToken, ValueTask> action,
        SidebarControlOptions? options)
    {
        return async () =>
        {
            try
            {
                await action(CancellationToken.None);
            }
            catch (Exception exception)
            {
                options?.OnActionError?.Invoke(exception);
                if (options?.OnActionError is null)
                {
                    throw;
                }
            }
        };
    }
}

