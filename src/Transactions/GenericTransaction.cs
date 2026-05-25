using IATec.Shared.Domain.Contracts.Transactions;
using Microsoft.EntityFrameworkCore;

namespace IATec.Shared.EF.Repository.Transactions;

/// <summary>
/// Provides a generic abstraction for managing database transactions using Entity Framework Core.
/// </summary>
/// <remarks>
/// This class wraps the underlying <see cref="DbContext.Database"/> transaction APIs
/// and should be used within a Unit of Work pattern to ensure atomic operations.
/// </remarks>
public abstract class GenericTransaction(DbContext dbContext) : ITransaction
{
    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    public void BeginTransaction()
    {
        dbContext.Database.BeginTransaction();
    }

    /// <summary>
    /// Commits the current database transaction.
    /// </summary>
    public void CommitTransaction()
    {
        dbContext.Database.CommitTransaction();
    }

    /// <summary>
    /// Rolls back the current database transaction.
    /// </summary>
    public void RollbackTransaction()
    {
        dbContext.Database.RollbackTransaction();
    }
}