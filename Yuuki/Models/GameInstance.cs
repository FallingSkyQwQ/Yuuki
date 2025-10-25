using System;

namespace Yuuki.Models;

/// <summary>
/// Represents a game instance with specific version and mod configuration
/// </summary>
public class GameInstance
{
    /// <summary>
    /// Unique identifier for the instance
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Display name for the instance
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Minecraft version (e.g., "1.20.1")
    /// </summary>
    public string MinecraftVersion { get; set; } = string.Empty;

    /// <summary>
    /// Mod loader type if any
    /// </summary>
    public ModLoaderType? ModLoader { get; set; }

    /// <summary>
    /// Mod loader version if mod loader is used
    /// </summary>
    public string? ModLoaderVersion { get; set; }

    /// <summary>
    /// Timestamp when the instance was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the instance was last played
    /// </summary>
    public DateTime? LastPlayed { get; set; }

    /// <summary>
    /// Total playtime in seconds
    /// </summary>
    public long TotalPlayTime { get; set; }

    /// <summary>
    /// Maximum memory allocation in MB
    /// </summary>
    public int MaxMemoryMB { get; set; } = 2048;

    /// <summary>
    /// Minimum memory allocation in MB
    /// </summary>
    public int MinMemoryMB { get; set; } = 512;

    /// <summary>
    /// Custom JVM arguments
    /// </summary>
    public string? CustomJvmArgs { get; set; }

    /// <summary>
    /// Game window width
    /// </summary>
    public int? WindowWidth { get; set; }

    /// <summary>
    /// Game window height
    /// </summary>
    public int? WindowHeight { get; set; }
}
