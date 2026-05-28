using System.Text.Json;

while (await Console.In.ReadLineAsync() is { } line)
{
    using var document = JsonDocument.Parse(line);
    var type = document.RootElement.GetProperty("Type").GetString();
    var pluginId = document.RootElement.TryGetProperty("PluginId", out var pluginIdElement)
        ? pluginIdElement.GetString()
        : null;

    if (string.Equals(type, "activate", StringComparison.Ordinal))
    {
        await Console.Out.WriteLineAsync(JsonSerializer.Serialize(new
        {
            Type = "activated",
            PluginId = pluginId,
            Capabilities = new[] { "commands", "status", "diagnostics" },
        }));
        await Console.Out.FlushAsync();
        continue;
    }

    if (string.Equals(type, "shutdown", StringComparison.Ordinal))
    {
        await Console.Out.WriteLineAsync(JsonSerializer.Serialize(new { Type = "shutdownComplete" }));
        await Console.Out.FlushAsync();
        return;
    }

    await Console.Out.WriteLineAsync(JsonSerializer.Serialize(new { Type = "error", Message = "Unknown message." }));
    await Console.Out.FlushAsync();
}
