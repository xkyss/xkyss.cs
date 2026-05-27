namespace Mewoo.Abstractions.Logging;

public interface IMewooLogger
{
    event Action? Changed;

    IReadOnlyList<MewooLogEntry> Entries { get; }

    void Info(string source, string message);

    void Error(string source, string message, Exception? exception = null);
}

public sealed record MewooLogEntry(
    DateTimeOffset Timestamp,
    MewooLogLevel Level,
    string Source,
    string Message,
    string? Exception);

public enum MewooLogLevel
{
    Info,
    Error,
}

