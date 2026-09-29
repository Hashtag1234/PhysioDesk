using Microsoft.EntityFrameworkCore;
using PhysioDesk.Api.Features.Auth;

namespace PhysioDesk.Api.Common.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).HasMaxLength(200).IsRequired();
            entity.Property(u => u.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(u => u.RefreshTokenHash).HasMaxLength(200);
            // Role stored as int (default EF convention) — deliberately not a string
            // column to avoid any provider-specific enum-to-string quirks.
        });
    }
}
