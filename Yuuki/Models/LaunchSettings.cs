namespace Yuuki.Models;

/// <summary>
/// Launch settings for a game instance
/// </summary>
public class LaunchSettings
{
    /// <summary>
    /// Maximum memory in MB
    /// </summary>
    public int MaxMemoryMB { get; set; } = 2048;

    /// <summary>
    /// Minimum memory in MB
    /// </summary>
    public int MinMemoryMB { get; set; } = 512;

    /// <summary>
    /// Custom JVM arguments
    /// </summary>
    public string? CustomJvmArgs { get; set; }

    /// <summary>
    /// Custom game arguments
    /// </summary>
    public string? CustomGameArgs { get; set; }

    /// <summary>
    /// Window width (null for default)
    /// </summary>
    public int? WindowWidth { get; set; }

    /// <summary>
    /// Window height (null for default)
    /// </summary>
    public int? WindowHeight { get; set; }

    /// <summary>
    /// Whether to start in fullscreen
    /// </summary>
    public bool StartFullscreen { get; set; }

    /// <summary>
    /// Java executable path (null for auto-detect)
    /// </summary>
    public string? JavaPath { get; set; }

    /// <summary>
    /// Working directory override
    /// </summary>
    public string? WorkingDirectory { get; set; }
}
