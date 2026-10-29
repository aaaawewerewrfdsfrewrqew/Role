using Microsoft.EntityFrameworkCore;
using RoleAuth.Api.Models;

namespace RoleAuth.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserToken> UserTokens => Set<UserToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Name).IsRequired().HasMaxLength(50);
            e.HasIndex(r => r.Name).IsUnique();          // ім'я ролі унікальне
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Username).IsRequired().HasMaxLength(50).UseCollation("NOCASE");
            e.Property(u => u.Email).IsRequired().HasMaxLength(256).UseCollation("NOCASE");
            e.Property(u => u.PasswordHash).IsRequired();
            e.HasIndex(u => u.Username).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();

            e.HasMany(u => u.Roles).WithMany(r => r.Users);  // many-to-many
        });

        modelBuilder.Entity<UserToken>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.TokenHash).IsRequired().HasMaxLength(64);
            e.Property(t => t.NewEmail).HasMaxLength(256);
            e.HasIndex(t => t.TokenHash).IsUnique();
            e.HasIndex(t => new { t.UserId, t.Type });
            e.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
