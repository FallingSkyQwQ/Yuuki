using System;
using System.Collections.Generic;

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
    /// Launch settings serialized as JSON
    /// </summary>
    public string? LaunchSettingsJson { get; set; }

    /// <summary>
    /// Icon/image path for this instance
    /// </summary>
    public string? IconPath { get; set; }

    /// <summary>
    /// Notes about this instance
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Installed mods for this instance
    /// </summary>
    public List<InstalledMod> InstalledMods { get; set; } = new();
}
