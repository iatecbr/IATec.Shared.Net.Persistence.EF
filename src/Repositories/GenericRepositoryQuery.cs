using System.Linq.Expressions;
using IATec.Shared.Domain.Contracts.Entities;
using IATec.Shared.Domain.Contracts.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

namespace IATec.Shared.EF.Repository.Repositories;

/// <summary>
/// Provides query construction capabilities for entities with support for eager-loading related data.
/// </summary>
public class GenericRepositoryQuery(DbContext dbContext) : IGenericRepositoryQuery
{
    /// <summary>
    /// Builds an <see cref="IQueryable{T}"/> including the specified related properties.
    /// </summary>
    /// <typeparam name="T">The entity type to query.</typeparam>
    /// <param name="includeProperties">Expressions representing the navigation properties to include.</param>
    /// <returns>An <see cref="IQueryable{T}"/> with the requested includes applied.</returns>
    public IQueryable<T> Query<T>(params Expression<Func<T, object>>[] includeProperties) where T : class, IEntity
    {
        var query = dbContext.Set<T>()
            .AsQueryable();

        query = includeProperties.Aggregate(query, (current, includeProperty)
            => current.Include(includeProperty));

        return query;
    }
}