using System.Linq.Expressions;

namespace IATec.Shared.EF.Repository.Extensions;

/// <summary>
/// Provides extension methods for building dynamically ordered queries.
/// </summary>
public static class QueryableExtension
{
    private const string Asc = "asc";

    /// <summary>
    /// Orders the query with primary sorting by <paramref name="includeProperties"/> (descending),
    /// then applies <paramref name="orderBy"/> as a secondary sort in the requested direction.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="orderDirection">The ordering direction for <paramref name="orderBy"/>: "asc" or "desc".</param>
    /// <param name="orderBy">The name of the secondary property to order by.</param>
    /// <param name="includeProperties">Primary properties for descending ordering.</param>
    /// <returns>An ordered queryable over <typeparamref name="T" />.</returns>
    public static IQueryable<T> Ordering<T>(this IQueryable<T> query, string orderDirection, string orderBy,
        params Expression<Func<T, object>>[] includeProperties)
    {
        var propertyExpression = BuildPropertyExpression<T>(orderBy);
        if (includeProperties.Length == 0)
        {   
            return orderDirection.Equals(Asc, StringComparison.CurrentCultureIgnoreCase)
                ? query.OrderBy(propertyExpression)
                : query.OrderByDescending(propertyExpression);
        }
        
        var firstExpression = includeProperties.FirstOrDefault();
        var resultExpression = includeProperties.Skip(1).ToArray();
        
        if (orderDirection.Equals(Asc, StringComparison.CurrentCultureIgnoreCase))
        {
            return query
                .OrderByDescending(firstExpression!)
                .OrderByThenDescending(resultExpression)
                .ThenBy(propertyExpression);
        }

        return query
            .OrderByDescending(firstExpression!)
            .OrderByThenDescending(resultExpression)
            .ThenByDescending(propertyExpression);
    }
    
    /// <summary>
    /// Applies a chained descending secondary ordering to an already ordered query.
    /// </summary>
    private static IOrderedQueryable<T> OrderByThenDescending<T>(this IOrderedQueryable<T> query, 
        params Expression<Func<T, object>>[] includeProperties)
    {
        return includeProperties.Aggregate(query, (queryable, expression) => queryable.ThenByDescending(expression));
    }

    /// <summary>
    /// Builds a property-access expression from a dot-separated property path string.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="orderBy">Dot-separated path of the property (e.g., "Customer.Name").</param>
    /// <returns>An expression that accesses the specified property.</returns>
    private static Expression<Func<T, object>> BuildPropertyExpression<T>(string orderBy)
    {
        var parameter = Expression.Parameter(typeof(T), "property");
        
        var member = orderBy
            .Split('.')
            .Aggregate((Expression)parameter, Expression.PropertyOrField);
        
        Expression conversion = Expression.Convert(member, typeof(object));
        return Expression.Lambda<Func<T, object>>(conversion, parameter);
    }
}