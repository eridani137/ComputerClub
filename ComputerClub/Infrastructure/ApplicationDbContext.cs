using ComputerClub.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ComputerClub.Infrastructure;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public DbSet<ComputerEntity> Computers => Set<ComputerEntity>();
    public DbSet<ClientEntity> Clients => Set<ClientEntity>();
    public DbSet<TariffEntity> Tariffs => Set<TariffEntity>();
    public DbSet<SessionEntity> Sessions => Set<SessionEntity>();
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public ApplicationDbContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ComputerEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}