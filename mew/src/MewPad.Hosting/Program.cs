namespace MewPad.Hosting;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewPad.Core.Shell;
using MewPad.Hosting.Extensions;
using MewPad.Hosting.Plugins;
using System.Diagnostics;

/// <summary>
/// Application entry point for MewPad.
/// </summary>
internal class Program
{
    private const string HostVersion = "0.1.0-preview";

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

        // Load runtime plugins from default and development fallback directories.
        var defaultPluginsDir = Path.Combine(AppContext.BaseDirectory, "plugins");
        var pluginDirectories = new List<string>
        {
            defaultPluginsDir
        };

        var repoRoot = FindRepoRoot(Directory.GetCurrentDirectory());
        if (repoRoot != null && !HasPluginArtifacts(defaultPluginsDir))
        {
            pluginDirectories.Add(Path.Combine(repoRoot, ".build", "QuickLaunch.Plugin", "bin", "Debug", "net10.0"));
            pluginDirectories.Add(Path.Combine(repoRoot, ".build", "QuickLaunch.Plugin", "bin", "Release", "net10.0"));
        }

        var disabledPluginIds = PluginConfig.GetDisabledPluginIds(shell.Configuration);
        var totalLoaded = 0;
        var totalFailed = 0;
        var allErrors = new List<string>();
        var allItems = new List<PluginLoadItem>();

        foreach (var dir in pluginDirectories.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var summary = PluginLoader.LoadFromDirectory(shell, dir, new PluginLoadOptions(HostVersion, disabledPluginIds));
            totalLoaded += summary.LoadedPlugins;
            totalFailed += summary.FailedPlugins;
            allItems.AddRange(summary.Items);
            foreach (var err in summary.Errors)
                allErrors.Add($"{Path.GetFileName(dir)}: {err}");
        }

        shell.Settings.RegisterCategory(new PluginManagementSettings(
            shell.Configuration,
            new PluginLoadSummary("multi", totalLoaded, totalFailed, allErrors, allItems)));

        if (totalLoaded > 0)
        {
            shell.RegisterStatusBarItem(new StatusBarItem(
                "plugins-count",
                () => new Label { Text = $"  Plugins {totalLoaded}  ", FontSize = 11, VerticalAlignment = VerticalAlignment.Center },
                StatusBarSlot.Right,
                Priority: 5));
        }

        if (totalLoaded > 0 || totalFailed > 0)
        {
            Console.WriteLine($"[Plugins] Loaded={totalLoaded}, Failed={totalFailed}");
            foreach (var error in allErrors)
                Console.WriteLine($"[Plugins] {error}");
        }

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

    private static string? FindRepoRoot(string startDirectory)
    {
        var current = new DirectoryInfo(startDirectory);
        while (current != null)
        {
            var hasSolution = File.Exists(Path.Combine(current.FullName, "MewPad.slnx"));
            var hasSrc = Directory.Exists(Path.Combine(current.FullName, "src", "MewPad.Hosting"));
            if (hasSolution && hasSrc)
                return current.FullName;

            current = current.Parent;
        }

        return null;
    }

    private static bool HasPluginArtifacts(string pluginsDirectory)
    {
        if (!Directory.Exists(pluginsDirectory))
            return false;

        var manifestExists = Directory.GetFiles(pluginsDirectory, "plugin.json", SearchOption.AllDirectories).Length > 0;
        var dllExists = Directory.GetFiles(pluginsDirectory, "*.dll", SearchOption.AllDirectories).Length > 0;
        return manifestExists || dllExists;
    }
}