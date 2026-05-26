namespace Mewoo.Abstractions.Commands;

public sealed record MewooCommandDescriptor(
    string Id,
    string OwnerPluginId,
    string Title,
    string? Category,
    string? Icon,
    string? DefaultKeyBinding,
    Func<IMewooCommandContext, CancellationToken, ValueTask> ExecuteAsync,
    Func<IMewooCommandContext, bool>? CanExecute = null);

public interface IMewooCommandContext
{
    IServiceProvider Services { get; }

    IWorkbenchService Workbench { get; }
}

