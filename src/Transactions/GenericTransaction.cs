using IATec.Shared.Domain.Contracts.Dispatcher;
using IATec.Shared.Domain.Contracts.Transactions;
using IATec.Shared.EF.Repository.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace IATec.Shared.EF.Repository.Transactions;

/// <summary>
/// Provides a generic abstraction for managing database transactions using Entity Framework Core.
/// </summary>
/// <remarks>
/// This class wraps the underlying <see cref="DbContext.Database"/> transaction APIs
/// and should be used within a Unit of Work pattern to ensure atomic operations.
/// It also coordinates audit log dispatch with the transaction outcome: logs captured during the
/// transaction are dispatched only when it commits and discarded when it rolls back.
/// The constructor signature is intentionally unchanged so existing derived classes keep working.
/// </remarks>
public abstract class GenericTransaction(DbContext dbContext) : ITransaction
{
    /// <summary>
    /// Begins a new database transaction and enlists the audit log buffer so that logs produced
    /// during the transaction are deferred until commit.
    /// </summary>
    public void BeginTransaction()
    {
        dbContext.Database.BeginTransaction();
        AuditLogBuffer.For(dbContext).EnlistTransaction();
    }

    /// <summary>
    /// Commits the current database transaction and dispatches all audit logs captured during it.
    /// </summary>
    public void CommitTransaction()
    {
        dbContext.Database.CommitTransaction();

        // The write is durable now: dispatch the buffered logs.
        var dispatcher = ResolveLogDispatcher();
        if (dispatcher is not null)
            AuditLogBuffer.For(dbContext).FlushAsync(dispatcher).GetAwaiter().GetResult();
        else
            AuditLogBuffer.For(dbContext).Discard();
    }

    /// <summary>
    /// Rolls back the current database transaction and discards any audit logs captured during it.
    /// </summary>
    public void RollbackTransaction()
    {
        dbContext.Database.RollbackTransaction();
        AuditLogBuffer.For(dbContext).Discard();
    }

    /// <summary>
    /// Resolves the <see cref="ILogDispatcher"/> from the application service provider associated
    /// with the current <see cref="DbContext"/>. Returns null if it is not registered.
    /// </summary>
    private ILogDispatcher? ResolveLogDispatcher()
    {
        return dbContext.GetInfrastructure().GetService<ILogDispatcher>();
    }
}
