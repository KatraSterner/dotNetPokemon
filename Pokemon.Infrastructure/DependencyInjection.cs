using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pokemon.Infrastructure.Data;
using Pokemon.Infrastructure.Repositories;

namespace Pokemon.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<PokemonDbContext>(options =>
        {
            options.UseNpgsql(
                config.GetConnectionString("NeonDb"));
        });

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}