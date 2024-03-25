using IATec.Shared.Domain.Contracts.Dispatcher;
using IATec.Shared.Domain.Contracts.Entities;
using IATec.Shared.Domain.Contracts.Repositories.Generic;
using IATec.Shared.Domain.Identifies.Logging;
using Microsoft.EntityFrameworkCore;

namespace IATec.Shared.EF.Repository.Repositories.Generic;

public class WriteRepository<T>(
    DbContext dbWriteContext,
    ILogDispatcher logDispatcher)
    : ReadRepository<T>(dbWriteContext), IWriteRepository<T>
    where T : class, IEntity
{
    private readonly DbSet<T> _dbSet = dbWriteContext.Set<T>();
    private readonly DbContext _dbContext = dbWriteContext;

    protected async Task SaveLogAsync(dynamic entity, LogActionType action)
    {
        await _dbContext.SaveChangesAsync();

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
        await _dbSet.AddAsync(entity);

        await SaveLogAsync(entity, LogActionType.Added);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);

        foreach (var entity in entities) await SaveLogAsync(entity, LogActionType.Added);
    }

    public async Task RemoveAsync(T entity)
    {
        _dbSet.Remove(entity);

        await SaveLogAsync(entity, LogActionType.Deleted);
    }

    public async Task RemoveRangeAsync(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);

        foreach (var entity in entities) await SaveLogAsync(entity, LogActionType.Deleted);
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);

        await SaveLogAsync(entity, LogActionType.Modified);
    }

    public async Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);

        foreach (var entity in entities) await SaveLogAsync(entity, LogActionType.Modified);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken) > 0;
    }
}