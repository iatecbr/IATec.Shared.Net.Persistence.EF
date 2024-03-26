using System.Linq.Expressions;
using IATec.Shared.Domain.Contracts.Entities;
using IATec.Shared.Domain.Contracts.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

namespace IATec.Shared.EF.Repository.Repositories;

// ReSharper disable once SuggestBaseTypeForParameterInConstructor
internal class GenericRepositoryQuery(DbContext dbContext) : IGenericRepositoryQuery
{
    public IQueryable<T> Query<T>(params Expression<Func<T, object>>[] includeProperties) where T : class, IEntity
    {
        var query = dbContext.Set<T>()
            .AsQueryable();

        query = includeProperties.Aggregate(query, (current, includeProperty)
            => current.Include(includeProperty));

        return query;
    }
}