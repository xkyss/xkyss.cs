namespace Ks.Core.Repositories;

public interface IRepository<TEntity>
{
    Task<TEntity> InsertAsync(TEntity transaction, bool autoSave = false, CancellationToken cancellationToken = default);

    Task<TEntity> UpdateAsync(TEntity transaction, bool autoSave = false, CancellationToken cancellationToken = default);
}

public interface IRepository<TEntity, TId> : IRepository<TEntity>
{
    Task<TEntity> GetAsync(TId id, CancellationToken cancellationToken = default);

    Task DeleteAsync(TId id, CancellationToken cancellationToken = default);
}
