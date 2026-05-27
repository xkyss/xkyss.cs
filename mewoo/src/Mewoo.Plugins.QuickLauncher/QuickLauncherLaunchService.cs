using System.Diagnostics;

namespace Mewoo.Plugins.QuickLauncher;

internal static class QuickLauncherLaunchService
{
    public static void Run(QuickLauncherItem item)
    {
        switch (item.Kind)
        {
            case QuickLauncherItemKind.Url:
            case QuickLauncherItemKind.File:
                StartShell(item.Target, item.Arguments);
                return;

            case QuickLauncherItemKind.Executable:
                StartShell(item.Target, item.Arguments);
                return;

            case QuickLauncherItemKind.Script:
                StartScript(item);
                return;

            default:
                throw new InvalidOperationException($"Unsupported launcher kind '{item.Kind}'.");
        }
    }

    public static void OpenConfig()
    {
        StartShell(QuickLauncherConfig.GetDefaultConfigPath(), null);
    }

    private static void StartScript(QuickLauncherItem item)
    {
        if (string.Equals(Path.GetExtension(item.Target), ".ps1", StringComparison.OrdinalIgnoreCase))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{item.Target}\" {item.Arguments}".TrimEnd(),
                UseShellExecute = true,
            });
            return;
        }

        StartShell(item.Target, item.Arguments);
    }

    private static void StartShell(string target, string? arguments)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = target,
            Arguments = arguments ?? string.Empty,
            UseShellExecute = true,
        });
    }
}

