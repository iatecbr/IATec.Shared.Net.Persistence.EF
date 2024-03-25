using IATec.Shared.Domain.Contracts.Transactions;
using Microsoft.EntityFrameworkCore;

namespace IATec.Shared.EF.Repository.Transactions;

public abstract class GenericTransaction(DbContext dbContext) : ITransaction
{
    public void BeginTransaction()
    {
        dbContext.Database.BeginTransaction();
    }

    public void CommitTransaction()
    {
        dbContext.Database.CommitTransaction();
    }

    public void RollbackTransaction()
    {
        dbContext.Database.RollbackTransaction();
    }
}