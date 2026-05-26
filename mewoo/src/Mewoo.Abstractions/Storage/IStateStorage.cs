namespace Mewoo.Abstractions.Storage;

public interface IStateStorage
{
    ValueTask<T?> ReadJsonAsync<T>(string key, CancellationToken cancellationToken = default);

    ValueTask WriteJsonAsync<T>(string key, T value, CancellationToken cancellationToken = default);
}

