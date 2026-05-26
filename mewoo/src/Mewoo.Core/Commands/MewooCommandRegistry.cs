using Mewoo.Abstractions.Commands;

namespace Mewoo.Core.Commands;

public sealed class MewooCommandRegistry
{
    private readonly Dictionary<string, MewooCommandDescriptor> _commands = new(StringComparer.Ordinal);

    public IReadOnlyCollection<MewooCommandDescriptor> Commands => _commands.Values;

    public void Register(MewooCommandDescriptor descriptor)
    {
        if (_commands.ContainsKey(descriptor.Id))
        {
            throw new InvalidOperationException($"Command id '{descriptor.Id}' is already registered.");
        }

        _commands.Add(descriptor.Id, descriptor);
    }

    public void UnregisterOwner(string ownerPluginId)
    {
        var ownedCommandIds = _commands.Values
            .Where(command => command.OwnerPluginId == ownerPluginId)
            .Select(command => command.Id)
            .ToArray();

        foreach (var commandId in ownedCommandIds)
        {
            _commands.Remove(commandId);
        }
    }

    public bool Contains(string commandId) => _commands.ContainsKey(commandId);

    public async ValueTask ExecuteAsync(
        string commandId,
        IMewooCommandContext context,
        CancellationToken cancellationToken = default)
    {
        if (!_commands.TryGetValue(commandId, out var descriptor))
        {
            throw new KeyNotFoundException($"Command id '{commandId}' is not registered.");
        }

        if (descriptor.CanExecute?.Invoke(context) == false)
        {
            return;
        }

        await descriptor.ExecuteAsync(context, cancellationToken);
    }
}
