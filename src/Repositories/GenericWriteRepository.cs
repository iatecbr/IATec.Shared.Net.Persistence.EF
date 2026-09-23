using IATec.Shared.Domain.Contracts.Dispatcher;
using IATec.Shared.Domain.Contracts.Entities;
using IATec.Shared.Domain.Contracts.Repositories.Generic;
using IATec.Shared.Domain.Identifies.Logging;
using IATec.Shared.EF.Repository.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IATec.Shared.EF.Repository.Repositories;

/// <summary>
/// Provides write operations for a specific entity type, including add, update, remove,
/// and automatic audit logging dispatch.
/// </summary>
/// <remarks>
/// Audit logs are dispatched only after changes are actually persisted to the database.
/// The affected entities and their content are captured from the change tracker before saving.
/// When an ambient transaction is active, dispatch is deferred to the audit log buffer and only
/// happens when the transaction commits; if it rolls back, the buffered logs are discarded.
/// </remarks>
/// <typeparam name="T">The entity type handled by the repository.</typeparam>
public abstract class GenericWriteRepository<T>(
    DbContext dbWriteContext,
    ILogDispatcher logDispatcher)
    : GenericReadRepository<T>(dbWriteContext), IWriteRepository<T>
    where T : class, IEntity
{
    protected new readonly DbSet<T> DbSet = dbWriteContext.Set<T>();
    protected readonly DbContext DbContext = dbWriteContext;

    private static readonly IReadOnlyDictionary<EntityState, LogActionType> StateToAction =
        new Dictionary<EntityState, LogActionType>
        {
            [EntityState.Added] = LogActionType.Added,
            [EntityState.Modified] = LogActionType.Modified,
            [EntityState.Deleted] = LogActionType.Deleted
        };

    /// <summary>
    /// Adds a single entity to the change tracker. The audit log is dispatched only after
    /// a successful <see cref="SaveChangesAsync"/>.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    public async Task AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);
    }

    /// <summary>
    /// Adds a collection of entities to the change tracker. Audit logs are dispatched only after
    /// a successful <see cref="SaveChangesAsync"/>.
    /// </summary>
    /// <param name="entities">The collection of entities to add.</param>
    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await DbSet.AddRangeAsync(entities);
    }

    /// <summary>
    /// Marks a single entity for removal in the change tracker. The audit log is dispatched only after
    /// a successful <see cref="SaveChangesAsync"/>.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    public Task RemoveAsync(T entity)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Marks a collection of entities for removal in the change tracker. Audit logs are dispatched only after
    /// a successful <see cref="SaveChangesAsync"/>.
    /// </summary>
    /// <param name="entities">The collection of entities to remove.</param>
    public Task RemoveRangeAsync(IEnumerable<T> entities)
    {
        DbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Marks a single entity as modified in the change tracker. The audit log is dispatched only after
    /// a successful <see cref="SaveChangesAsync"/>.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    public Task UpdateAsync(T entity)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Marks a collection of entities as modified in the change tracker. Audit logs are dispatched only after
    /// a successful <see cref="SaveChangesAsync"/>.
    /// </summary>
    /// <param name="entities">The collection of entities to update.</param>
    public Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        DbSet.UpdateRange(entities);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Saves all changes made in the context to the database and dispatches an audit log
    /// for every entity that was actually persisted.
    /// </summary>
    /// <remarks>
    /// The set of affected entities and their log content are captured from the change tracker
    /// before persistence. If an ambient transaction is active, dispatch is deferred to the
    /// audit log buffer (flushed on commit); otherwise logs are dispatched immediately after a
    /// successful save.
    /// </remarks>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if any changes were persisted; otherwise <c>false</c>.</returns>
    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Capture the entities/state/content BEFORE saving: deleted content must be read before
        // the row is gone, and the tracker states reset to Unchanged after a successful save.
        var pendingLogs = CapturePendingLogs();

        var persisted = await DbContext.SaveChangesAsync(cancellationToken) > 0;

        if (pendingLogs.Count == 0)
            return persisted;

        // Resolve the owner AFTER saving so database-generated keys (identity) are already
        // assigned; for Added entities GetOwner() would otherwise return the default "0".
        var capturedLogs = MaterializeLogs(pendingLogs);

        var buffer = AuditLogBuffer.For(DbContext);
        buffer.Add(capturedLogs);

        var hasActiveTransaction =
            buffer.IsTransactionActive || DbContext.Database.CurrentTransaction is not null;

        // With an active transaction the write is only durable on commit, so defer dispatch
        // to the buffer, which is flushed by the transaction on commit and discarded on rollback.
        if (!hasActiveTransaction)
            await buffer.FlushAsync(logDispatcher, cancellationToken);

        return persisted;
    }

    /// <summary>
    /// Holds the audit information captured from the change tracker before persistence, while
    /// keeping a reference to the tracked entity so its owner can be resolved after the save,
    /// once database-generated keys have been assigned.
    /// </summary>
    /// <param name="Entity">The tracked entity the log refers to.</param>
    /// <param name="Source">The source type value of the entity.</param>
    /// <param name="Action">The action performed (added, modified, or deleted).</param>
    /// <param name="Content">Optional content to log. Null for delete actions.</param>
    private sealed record PendingLog(IEntity Entity, string Source, string Action, object? Content);

    /// <summary>
    /// Reads the change tracker and captures the pending audit information for each tracked
    /// <see cref="IEntity"/> in an Added, Modified, or Deleted state.
    /// </summary>
    /// <remarks>
    /// The content is materialized here because deleted rows must be read before persistence.
    /// The owner is NOT resolved yet: for Added entities the identity key is only assigned after
    /// <see cref="DbContext.SaveChangesAsync"/>, so owner resolution is deferred to
    /// <see cref="MaterializeLogs"/>. The captured entity reference is the same instance EF updates
    /// in place with the generated key.
    /// </remarks>
    private List<PendingLog> CapturePendingLogs()
    {
        var pendingLogs = new List<PendingLog>();

        foreach (var entry in DbContext.ChangeTracker.Entries())
        {
            if (entry.Entity is not IEntity entity) continue;

            var source = entity.GetSourceType();
            if (source is null) continue;

            var action = ResolveAction(entry);
            if (action is null) continue;

            var content = action != LogActionType.Deleted ? entity.GetLogContent() : null;

            pendingLogs.Add(new PendingLog(entity, source.Value, action.Value, content));
        }

        return pendingLogs;
    }

    /// <summary>
    /// Builds the immutable <see cref="AuditLog"/> snapshots from the captured pending logs,
    /// resolving each entity's owner after persistence so database-generated keys are reflected.
    /// </summary>
    private static List<AuditLog> MaterializeLogs(IReadOnlyCollection<PendingLog> pendingLogs)
    {
        var capturedLogs = new List<AuditLog>(pendingLogs.Count);

        foreach (var pending in pendingLogs)
            capturedLogs.Add(new AuditLog(pending.Source, pending.Entity.GetOwner(), pending.Action, pending.Content));

        return capturedLogs;
    }

    /// <summary>
    /// Resolves the audit action for a root entity entry.
    /// </summary>
    /// <remarks>
    /// Besides the direct Added/Modified/Deleted states, an entity is also considered modified when
    /// it appears as Unchanged but owns a value object (owned type) that was changed. Replacing an
    /// owned instance leaves the root Unchanged while the owned entries are marked Added/Deleted, so
    /// this method attributes that change back to the owning entity.
    /// </remarks>
    private static LogActionType? ResolveAction(EntityEntry entry)
    {
        if (StateToAction.TryGetValue(entry.State, out var action))
            return action;

        if (entry.State == EntityState.Unchanged && HasModifiedOwnedMembers(entry))
            return LogActionType.Modified;

        return null;
    }

    /// <summary>
    /// Determines whether any owned reference or collection of the given entry has pending changes.
    /// </summary>
    private static bool HasModifiedOwnedMembers(EntityEntry entry)
    {
        // Owned value objects (e.g. mapped via OwnsOne) surface as owned reference entries.
        // Replacing the owned instance marks these targets as Added/Deleted/Modified while the
        // owning root stays Unchanged, so inspecting them detects the change.
        foreach (var reference in entry.References)
        {
            if (reference.TargetEntry is { } target && target.Metadata.IsOwned() && IsChanged(target))
                return true;
        }

        return false;
    }

    private static bool IsChanged(EntityEntry entry)
    {
        return entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted;
    }
}
