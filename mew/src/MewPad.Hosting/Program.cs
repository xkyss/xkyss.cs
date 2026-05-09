namespace MewPad.Hosting;

using Aprillz.MewUI;
using MewPad.Core.Shell;
using MewPad.Hosting.Extensions;

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
        shell.RegisterActivity(new ExplorerActivity());
        shell.RegisterActivity(new SearchActivity());
        shell.RegisterActivity(new WelcomeActivity());
        shell.Settings.RegisterCategory(new AppearanceSettings(shell.Theme));
        shell.Settings.RegisterCategory(new LanguageSettings(shell.Localization));

        // Create main window
        var window = AppWindowBuilder.CreateMainWindow(shell);

        // Run application
        Application.Run(window);
    }
}