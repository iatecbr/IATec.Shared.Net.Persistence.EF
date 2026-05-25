using IATec.Shared.Domain.Contracts.Repositories.Generic;
using IATec.Shared.EF.Repository.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IATec.Shared.EF.Repository.Configurations;

/// <summary>
/// Extension methods for registering EF Repository services into the dependency injection container.
/// </summary>
public static class PersistenceExtension
{
    /// <summary>
    /// Adds generic repository query services to the service collection using the specified <see cref="DbContext"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of <see cref="DbContext"/> to register.</typeparam>
    /// <param name="services">The service collection.</param>
    public static void AddAdditionalPersistenceData<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddScoped<IGenericRepositoryQuery>(provider =>
        {
            var context = provider.GetRequiredService<TContext>();
            return new GenericRepositoryQuery(context);
        });
    }
}