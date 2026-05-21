using IATec.Shared.Domain.Contracts.Entities;
using IATec.Shared.Domain.Contracts.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

namespace IATec.Shared.EF.Repository.Repositories;

/// <summary>
/// Provides basic read-only data access operations for a specific entity type.
/// </summary>
/// <typeparam name="T">The entity type handled by the repository.</typeparam>
public abstract class GenericReadRepository<T>(DbContext dbReadContext) : IReadRepository<T>
    where T : class, IEntity
{
    protected readonly DbSet<T> DbSet = dbReadContext.Set<T>();

    /// <summary>
    /// Retrieves all entities of type <typeparamref name="T"/> from the database.
    /// </summary>
    /// <returns>A list containing all entities.</returns>
    public async Task<List<T>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }

    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <returns>The entity if found; otherwise, <c>null</c>.</returns>
    public async Task<T?> GetByIdAsync(int id)
    {
        return await DbSet.FindAsync(id);
    }
}