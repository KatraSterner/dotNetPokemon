using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace Pokemon.Infrastructure.Data;

public class PokemonDbContext
    : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public PokemonDbContext(DbContextOptions<PokemonDbContext> options)
        : base(options) { }

    // Your app tables
    public DbSet<PokemonEntity> Pokemons => Set<PokemonEntity>();
    public DbSet<UserAuditLog> AuditLogs => Set<UserAuditLog>();
}