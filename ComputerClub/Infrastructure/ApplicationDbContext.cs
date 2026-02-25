using ComputerClub.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ComputerClub.Infrastructure;

public class ApplicationDbContext : IdentityDbContext<ComputerClubIdentity, IdentityRole<int>, int>
{
    public DbSet<ComputerEntity> Computers => Set<ComputerEntity>();
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
        
        builder.Entity<ComputerClubIdentity>(entity =>
        {
            entity.Property(e => e.FullName).HasMaxLength(200);
        });

        builder.Entity<ComputerEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}