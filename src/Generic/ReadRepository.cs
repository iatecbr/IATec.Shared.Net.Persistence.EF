using IATec.Shared.Domain.Contracts.Entities;
using IATec.Shared.Domain.Contracts.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

namespace IATec.Shared.EF.Repository.Generic;

public class ReadRepository<T>(DbContext dbContext) : IReadRepository<T>
    where T : class, IEntity
{
    private readonly DbSet<T> _dbSet = dbContext.Set<T>();

    public async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }
}