using IATec.Shared.Domain.Contracts.Dispatcher;
using IATec.Shared.Domain.Contracts.Entities;
using IATec.Shared.Domain.Contracts.Repositories.Generic;
using IATec.Shared.Domain.Identifies.Logging;
using Microsoft.EntityFrameworkCore;

namespace IATec.Shared.EF.Repository.Repositories;

public abstract class GenericWriteRepository<T>(
    DbContext dbWriteContext,
    ILogDispatcher logDispatcher)
    : GenericReadRepository<T>(dbWriteContext), IWriteRepository<T>
    where T : class, IEntity
{
    protected new readonly DbSet<T> DbSet = dbWriteContext.Set<T>();
    protected readonly DbContext DbContext = dbWriteContext;

    protected async Task SaveLogAsync(dynamic entity, LogActionType action)
    {
        await DbContext.SaveChangesAsync();

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

    public async Task AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);

        await SaveLogAsync(entity, LogActionType.Added);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await DbSet.AddRangeAsync(entities);

        foreach (var entity in entities) await SaveLogAsync(entity, LogActionType.Added);
    }

    public async Task RemoveAsync(T entity)
    {
        DbSet.Remove(entity);

        await SaveLogAsync(entity, LogActionType.Deleted);
    }

    public async Task RemoveRangeAsync(IEnumerable<T> entities)
    {
        DbSet.RemoveRange(entities);

        foreach (var entity in entities) await SaveLogAsync(entity, LogActionType.Deleted);
    }

    public async Task UpdateAsync(T entity)
    {
        DbSet.Update(entity);

        await SaveLogAsync(entity, LogActionType.Modified);
    }

    public async Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        DbSet.UpdateRange(entities);

        foreach (var entity in entities) await SaveLogAsync(entity, LogActionType.Modified);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.SaveChangesAsync(cancellationToken) > 0;
    }
}