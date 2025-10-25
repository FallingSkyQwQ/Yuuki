using System;

namespace Yuuki.Models;

/// <summary>
/// Represents an installed mod in a game instance
/// </summary>
public class InstalledMod
{
    /// <summary>
    /// Primary key
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Game instance ID this mod belongs to
    /// </summary>
    public string GameInstanceId { get; set; } = string.Empty;

    /// <summary>
    /// Mod ID from the platform
    /// </summary>
    public string ModId { get; set; } = string.Empty;

    /// <summary>
    /// Mod name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Installed version
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// File name of the mod JAR
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the mod is currently enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Platform where this mod came from
    /// </summary>
    public ModPlatform Platform { get; set; }

    /// <summary>
    /// When this mod was installed
    /// </summary>
    public DateTime InstalledAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Latest available version (for update checking)
    /// </summary>
    public string? LatestVersion { get; set; }

    /// <summary>
    /// Whether an update is available
    /// </summary>
    public bool HasUpdate { get; set; }

    /// <summary>
    /// Navigation property to game instance
    /// </summary>
    public GameInstance? GameInstance { get; set; }
}
