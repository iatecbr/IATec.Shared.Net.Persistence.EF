using System.Linq.Expressions;

namespace IATec.Shared.EF.Repository.Extensions;

public static class QueryableExtension
{
    private const string Asc = "asc";
    
    /// <summary>
    /// Ordering
    /// </summary>
    /// <param name="query"></param>
    /// <param name="orderDirection">Order sent from UI (ASC or DESC)</param>
    /// <param name="orderBy">Property sent from UI</param>
    /// <param name="includeProperties">Mandatory properties - DESC</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
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
    
    private static IOrderedQueryable<T> OrderByThenDescending<T>(this IOrderedQueryable<T> query, 
        params Expression<Func<T, object>>[] includeProperties)
    {
        return includeProperties.Aggregate(query, (queryable, expression) => queryable.ThenByDescending(expression));
    }

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