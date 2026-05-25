using IATec.Shared.Domain.Contracts.Dispatcher;
using IATec.Shared.Domain.Contracts.Entities;
using IATec.Shared.Domain.Contracts.Repositories.Generic;
using IATec.Shared.Domain.Identifies.Logging;
using Microsoft.EntityFrameworkCore;

namespace IATec.Shared.EF.Repository.Repositories;

/// <summary>
/// Provides write operations for a specific entity type, including add, update, remove,
/// and automatic audit logging dispatch.
/// </summary>
/// <typeparam name="T">The entity type handled by the repository.</typeparam>
public abstract class GenericWriteRepository<T>(
    DbContext dbWriteContext,
    ILogDispatcher logDispatcher)
    : GenericReadRepository<T>(dbWriteContext), IWriteRepository<T>
    where T : class, IEntity
{
    protected new readonly DbSet<T> DbSet = dbWriteContext.Set<T>();
    protected readonly DbContext DbContext = dbWriteContext;

    private async Task SaveLogAsync(dynamic entity, LogActionType action)
    {
        if (entity is not IEntity) return;

        var source = entity.GetSourceType();

        if (source == null) return;

        var contentObject = entity.GetLogContent();

        object? content = null;

        if (contentObject != null && action != LogActionType.Deleted) content = contentObject;

        _ = logDispatcher.DispatchAsync(
            source: source.Value,
            owner: entity.GetOwner(),
            action: action.Value,
            content: content);
    }

    /// <summary>
    /// Adds a single entity to the database and dispatches a log for the added action.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    public async Task AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);

        await SaveLogAsync(entity, LogActionType.Added);
    }

    /// <summary>
    /// Adds a collection of entities to the database and dispatches logs for each added action.
    /// </summary>
    /// <param name="entities">The collection of entities to add.</param>
    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await DbSet.AddRangeAsync(entities);

        foreach (var entity in entities) await SaveLogAsync(entity, LogActionType.Added);
    }

    /// <summary>
    /// Removes a single entity from the database and dispatches a log for the deleted action.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    public async Task RemoveAsync(T entity)
    {
        DbSet.Remove(entity);

        await SaveLogAsync(entity, LogActionType.Deleted);
    }

    /// <summary>
    /// Removes a collection of entities from the database and dispatches logs for each deleted action.
    /// </summary>
    /// <param name="entities">The collection of entities to remove.</param>
    public async Task RemoveRangeAsync(IEnumerable<T> entities)
    {
        DbSet.RemoveRange(entities);

        foreach (var entity in entities) await SaveLogAsync(entity, LogActionType.Deleted);
    }

    /// <summary>
    /// Updates a single entity in the database and dispatches a log for the modified action.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    public async Task UpdateAsync(T entity)
    {
        DbSet.Update(entity);

        await SaveLogAsync(entity, LogActionType.Modified);
    }

    /// <summary>
    /// Updates a collection of entities in the database and dispatches logs for each modified action.
    /// </summary>
    /// <param name="entities">The collection of entities to update.</param>
    public async Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        DbSet.UpdateRange(entities);

        foreach (var entity in entities) await SaveLogAsync(entity, LogActionType.Modified);
    }

    /// <summary>
    /// Saves all changes made in the context to the database.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if any changes were persisted; otherwise <c>false</c>.</returns>
    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.SaveChangesAsync(cancellationToken) > 0;
    }
}
