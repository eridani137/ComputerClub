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
            entity.ToTable("Users");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.FullName)
                .HasMaxLength(200);

            entity.Property(e => e.Balance)
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2);
        });

        builder.Entity<SessionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.TotalCost)
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2);
        });

        builder.Entity<TariffEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.PricePerHour)
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2);
        });

        builder.Entity<ComputerEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}