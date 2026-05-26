// <copyright>
// Copyright (c) 2026 MewPad Contributors
// </copyright>

using System.Diagnostics;

namespace MewPad.Core.Services;

/// <summary>
/// Lightweight logging abstraction for internal diagnostics.
/// </summary>
public interface ILogger
{
    void LogDebug(string message);
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? exception = null);
}

/// <summary>
/// Default <see cref="ILogger"/> that writes to <see cref="System.Diagnostics.Debug"/>.
/// </summary>
public class DebugLogger : ILogger
{
    private readonly string _category;

    public DebugLogger(string category) => _category = category;

    public void LogDebug(string message) => Debug.WriteLine($"[{_category}] {message}");
    public void LogInfo(string message) => Debug.WriteLine($"[{_category}] INFO: {message}");
    public void LogWarning(string message) => Debug.WriteLine($"[{_category}] WARN: {message}");
    public void LogError(string message, Exception? exception = null)
    {
        Debug.WriteLine($"[{_category}] ERROR: {message}");
        if (exception != null)
            Debug.WriteLine($"[{_category}] Exception: {exception}");
    }
}