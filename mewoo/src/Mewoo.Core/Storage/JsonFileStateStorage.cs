using System.Text.Json;
using Mewoo.Abstractions.Storage;

namespace Mewoo.Core.Storage;

public sealed class JsonFileStateStorage(string rootDirectory) : IStateStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    public async ValueTask<T?> ReadJsonAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var path = GetPath(key);
        if (!File.Exists(path))
        {
            return default;
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
    }

    public async ValueTask WriteJsonAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(rootDirectory);

        var path = GetPath(key);
        var tempPath = $"{path}.tmp";

        await using (var stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, value, JsonOptions, cancellationToken);
        }

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        File.Move(tempPath, path);
    }

    private string GetPath(string key)
    {
        var fileName = string.Concat(key.Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' or '.'
            ? ch
            : '_'));

        return Path.Combine(rootDirectory, $"{fileName}.json");
    }
}

