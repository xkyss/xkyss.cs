using Mewoo.Abstractions.Logging;

namespace Mewoo.Core.Logging;

public sealed class InMemoryMewooLogger : IMewooLogger
{
    private readonly List<MewooLogEntry> _entries = [];

    public event Action? Changed;

    public IReadOnlyList<MewooLogEntry> Entries => _entries;

    public void Info(string source, string message)
    {
        Add(new MewooLogEntry(DateTimeOffset.Now, MewooLogLevel.Info, source, message, null));
    }

    public void Error(string source, string message, Exception? exception = null)
    {
        Add(new MewooLogEntry(DateTimeOffset.Now, MewooLogLevel.Error, source, message, exception?.ToString()));
    }

    private void Add(MewooLogEntry entry)
    {
        _entries.Add(entry);
        Changed?.Invoke();
    }
}

