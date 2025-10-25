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
            entity.Property(e => e.CustomJvmArgs).HasMaxLength(1000);

            // Create index on Name for faster lookups
            entity.HasIndex(e => e.Name);

            // Create index on LastPlayed for sorting
            entity.HasIndex(e => e.LastPlayed);
        });
    }
}
