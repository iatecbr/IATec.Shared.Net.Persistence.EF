using IATec.Shared.Domain.Contracts.Entities;
using IATec.Shared.Domain.Contracts.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

namespace IATec.Shared.EF.Repository.Repositories;

/// <summary>
/// Provides write operations for a specific entity type: add, update, remove and persistence.
/// </summary>
/// <remarks>
/// This repository no longer captures or dispatches audit logs. Auditing is handled by the
/// optional audit outbox package (<c>IATec.Shared.Persistence.EF.AuditOutbox</c>), whose
/// save-changes interceptor captures the affected entities and writes the audit entries within the
/// same transaction as the business operation. The repository is therefore agnostic of auditing.
/// </remarks>
/// <typeparam name="T">The entity type handled by the repository.</typeparam>
public abstract class GenericWriteRepository<T>(DbContext dbWriteContext)
    : GenericReadRepository<T>(dbWriteContext), IWriteRepository<T>
    where T : class, IEntity
{
    protected new readonly DbSet<T> DbSet = dbWriteContext.Set<T>();
    protected readonly DbContext DbContext = dbWriteContext;

    /// <summary>
    /// Adds a single entity to the change tracker.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    public async Task AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);
    }

    /// <summary>
    /// Adds a collection of entities to the change tracker.
    /// </summary>
    /// <param name="entities">The collection of entities to add.</param>
    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await DbSet.AddRangeAsync(entities);
    }

    /// <summary>
    /// Marks a single entity for removal in the change tracker.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    public Task RemoveAsync(T entity)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Marks a collection of entities for removal in the change tracker.
    /// </summary>
    /// <param name="entities">The collection of entities to remove.</param>
    public Task RemoveRangeAsync(IEnumerable<T> entities)
    {
        DbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Marks a single entity as modified in the change tracker.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    public Task UpdateAsync(T entity)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Marks a collection of entities as modified in the change tracker.
    /// </summary>
    /// <param name="entities">The collection of entities to update.</param>
    public Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        DbSet.UpdateRange(entities);
        return Task.CompletedTask;
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
