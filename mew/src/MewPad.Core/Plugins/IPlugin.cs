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
    /// Registers plugin contributions to the shell.
    /// </summary>
    void Register(ShellContext shell);
}
