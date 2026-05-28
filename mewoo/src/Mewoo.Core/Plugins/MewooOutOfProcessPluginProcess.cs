using System.Diagnostics;
using System.Text.Json;

namespace Mewoo.Core.Plugins;

public sealed record MewooOutOfProcessPluginProcessOptions(
    string PluginId,
    string FileName,
    string Arguments);

public sealed record MewooOutOfProcessPluginActivationResult(
    bool Success,
    string? Message);

public sealed class MewooOutOfProcessPluginProcess : IAsyncDisposable
{
    private Process? _process;

    public bool IsRunning => _process is { HasExited: false };

    public async ValueTask<MewooOutOfProcessPluginActivationResult> ActivateAsync(
        MewooOutOfProcessPluginProcessOptions options,
        CancellationToken cancellationToken = default)
    {
        if (_process is { HasExited: false })
        {
            throw new InvalidOperationException("Out-of-process plugin host is already running.");
        }

        _process = StartProcess(options);
        await SendAsync(new MewooOutOfProcessMessage("activate", options.PluginId), cancellationToken);
        var response = await ReadLineAsync(cancellationToken);
        return response?.Contains("\"activated\"", StringComparison.Ordinal) == true
            ? new MewooOutOfProcessPluginActivationResult(true, response)
            : new MewooOutOfProcessPluginActivationResult(false, response);
    }

    public async ValueTask<bool> ShutdownAsync(CancellationToken cancellationToken = default)
    {
        if (_process is null)
        {
            return true;
        }

        if (!_process.HasExited)
        {
            await SendAsync(new MewooOutOfProcessMessage("shutdown", null), cancellationToken);
            await _process.WaitForExitAsync(cancellationToken);
        }

        return _process.HasExited;
    }

    public async ValueTask DisposeAsync()
    {
        if (_process is null)
        {
            return;
        }

        if (!_process.HasExited)
        {
            _process.Kill(entireProcessTree: true);
            await _process.WaitForExitAsync();
        }

        _process.Dispose();
    }

    private static Process StartProcess(MewooOutOfProcessPluginProcessOptions options)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = options.FileName,
            Arguments = options.Arguments,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        return Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start out-of-process plugin host.");
    }

    private async ValueTask SendAsync(MewooOutOfProcessMessage message, CancellationToken cancellationToken)
    {
        if (_process is null)
        {
            throw new InvalidOperationException("Out-of-process plugin host has not been started.");
        }

        await _process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(message).AsMemory(), cancellationToken);
        await _process.StandardInput.FlushAsync(cancellationToken);
    }

    private async ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
    {
        if (_process is null)
        {
            throw new InvalidOperationException("Out-of-process plugin host has not been started.");
        }

        return await _process.StandardOutput.ReadLineAsync(cancellationToken);
    }

    private sealed record MewooOutOfProcessMessage(string Type, string? PluginId);
}
