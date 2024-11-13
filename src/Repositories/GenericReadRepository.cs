using IATec.Shared.Domain.Contracts.Entities;
using IATec.Shared.Domain.Contracts.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

namespace IATec.Shared.EF.Repository.Repositories;

public abstract class GenericReadRepository<T>(DbContext dbReadContext) : IReadRepository<T>
    where T : class, IEntity
{
    protected readonly DbSet<T> DbSet = dbReadContext.Set<T>();

    public async Task<List<T>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await DbSet.FindAsync(id);
    }
}