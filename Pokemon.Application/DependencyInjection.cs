using Microsoft.Extensions.DependencyInjection;
using Pokemon.Application.Services;

namespace Pokemon.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<PokemonService>();
        
        return services;
    }
}