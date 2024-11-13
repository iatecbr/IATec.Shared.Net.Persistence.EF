using IATec.Shared.Domain.Contracts.Repositories.Generic;
using IATec.Shared.EF.Repository.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IATec.Shared.EF.Repository.Configurations;

public static class PersistenceExtension
{
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