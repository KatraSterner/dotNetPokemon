using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Pokemon.Infrastructure.Data;

public class PokemonDbContext
    : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public PokemonDbContext(DbContextOptions<PokemonDbContext> options)
        : base(options) { }

    // App tables
    public DbSet<Domain.Models.Pokemon> Pokemon => Set<Domain.Models.Pokemon>();
    public DbSet<UserAuditLog> AuditLogs => Set<UserAuditLog>();
}