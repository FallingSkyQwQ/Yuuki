using Microsoft.EntityFrameworkCore;
using Yuuki.Models;

namespace Yuuki.Data;

/// <summary>
/// Database context for Yuuki application
/// </summary>
public class YuukiDbContext : DbContext
{
    /// <summary>
    /// Game instances table
    /// </summary>
    public DbSet<GameInstance> GameInstances { get; set; } = null!;

    /// <summary>
    /// Installed mods table
    /// </summary>
    public DbSet<InstalledMod> InstalledMods { get; set; } = null!;

    /// <summary>
    /// User accounts table
    /// </summary>
    public DbSet<UserAccount> UserAccounts { get; set; } = null!;

    /// <summary>
    /// Initializes a new instance of the YuukiDbContext class
    /// </summary>
    /// <param name="options">Context options</param>
    public YuukiDbContext(DbContextOptions<YuukiDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure GameInstance entity
        modelBuilder.Entity<GameInstance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MinecraftVersion).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ModLoaderVersion).HasMaxLength(50);
            entity.Property(e => e.LaunchSettingsJson).HasMaxLength(4000);
            entity.Property(e => e.IconPath).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(2000);

            // Create index on Name for faster lookups
            entity.HasIndex(e => e.Name);

            // Create index on LastPlayed for sorting
            entity.HasIndex(e => e.LastPlayed);

            // Configure one-to-many relationship with InstalledMods
            entity.HasMany(e => e.InstalledMods)
                .WithOne(m => m.GameInstance)
                .HasForeignKey(m => m.GameInstanceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure InstalledMod entity
        modelBuilder.Entity<InstalledMod>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GameInstanceId).IsRequired();
            entity.Property(e => e.ModId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Version).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.LatestVersion).HasMaxLength(50);

            // Create index on GameInstanceId for faster queries
            entity.HasIndex(e => e.GameInstanceId);

            // Create index on ModId
            entity.HasIndex(e => e.ModId);

            // Create composite index for instance + enabled status
            entity.HasIndex(e => new { e.GameInstanceId, e.IsEnabled });
        });

        // Configure UserAccount entity
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Uuid).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.AccessToken).HasMaxLength(2000);
            entity.Property(e => e.RefreshToken).HasMaxLength(2000);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);

            // Create index on Uuid for faster lookups
            entity.HasIndex(e => e.Uuid).IsUnique();

            // Create index on Email
            entity.HasIndex(e => e.Email);

            // Create index on IsActive
            entity.HasIndex(e => e.IsActive);
        });
    }
}
