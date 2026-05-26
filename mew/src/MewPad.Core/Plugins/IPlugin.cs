namespace MewPad.Core.Plugins;

using MewPad.Core.Shell;

/// <summary>
/// Runtime plugin contract for MewPad.
/// Implement this interface in external plugin assemblies to register UI extensions.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Unique plugin identifier.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Minimum required host version for this plugin.
    /// </summary>
    string MinHostVersion { get; }

    /// <summary>
    /// IDs of plugins this plugin depends on (loaded before this one).
    /// </summary>
    string[] Dependencies { get; }

    /// <summary>
    /// Registers plugin contributions to the shell (required).
    /// </summary>
    void Register(ShellContext shell);

    /// <summary>
    /// Called after all plugins are registered. Override to perform async initialization.
    /// </summary>
    void Initialize(ShellContext shell) { }

    /// <summary>
    /// Called when the plugin is being unloaded. Override for cleanup.
    /// </summary>
    void Shutdown(ShellContext shell) { }
}
