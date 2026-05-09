namespace MewPad.Hosting;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewPad.Core.Shell;
using MewPad.Hosting.Extensions;
using System.Diagnostics;

/// <summary>
/// Application entry point for MewPad.
/// </summary>
internal class Program
{
    [System.STAThread]
    static void Main(string[] args)
    {
        // Ensure Win32 platform host and GDI backend are registered before running application.
        Win32Platform.Register();
        GdiBackend.Register();
        Application.SetDefaultGraphicsFactory("gdi");

        // Initialize shell context
        var shell = new ShellContext();

        // Register built-in extensions
        shell.Settings.RegisterCategory(new AppearanceSettings(shell.Theme));
        shell.Settings.RegisterCategory(new LanguageSettings(shell.Localization));
        shell.Settings.RegisterCategory(new AboutSettings());

        var gitBranch = GetGitBranch();

        // Register built-in StatusBar items
        shell.RegisterStatusBarItem(new StatusBarItem(
            "git-branch",
            () => new Label { Text = $"  ⎇ {gitBranch}  ", FontSize = 11, VerticalAlignment = VerticalAlignment.Center },
            StatusBarSlot.Left,
            Priority: 10));

        shell.RegisterStatusBarItem(new StatusBarItem(
            "errors",
            () => new Label { Text = "⊗ 0  ⚠ 0  ", FontSize = 11, VerticalAlignment = VerticalAlignment.Center },
            StatusBarSlot.Left,
            Priority: 20));

        shell.RegisterStatusBarItem(new StatusBarItem(
            "encoding",
            () => new Label { Text = "  UTF-8  ", FontSize = 11, VerticalAlignment = VerticalAlignment.Center },
            StatusBarSlot.Right,
            Priority: 10));

        // Create main window
        var window = AppWindowBuilder.CreateMainWindow(shell);

        // Run application
        Application.Run(window);
    }

    private static string GetGitBranch()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "branch --show-current",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = AppContext.BaseDirectory,
            };

            using var process = Process.Start(startInfo);
            if (process == null)
                return "main";

            var branch = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit(2000);
            return string.IsNullOrWhiteSpace(branch) ? "main" : branch;
        }
        catch
        {
            return "main";
        }
    }
}