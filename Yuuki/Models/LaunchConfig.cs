using System;
using System.Collections.Generic;

namespace Yuuki.Models;

/// <summary>
/// Launch configuration for a game instance
/// </summary>
public class LaunchConfig
{
    /// <summary>
    /// Java executable path
    /// </summary>
    public string JavaPath { get; set; } = "java";

    /// <summary>
    /// Maximum memory allocation in MB
    /// </summary>
    public int MaxMemoryMB { get; set; } = 2048;

    /// <summary>
    /// Minimum memory allocation in MB
    /// </summary>
    public int MinMemoryMB { get; set; } = 512;

    /// <summary>
    /// Game window width
    /// </summary>
    public int WindowWidth { get; set; } = 854;

    /// <summary>
    /// Game window height
    /// </summary>
    public int WindowHeight { get; set; } = 480;

    /// <summary>
    /// Whether to start in fullscreen
    /// </summary>
    public bool Fullscreen { get; set; } = false;

    /// <summary>
    /// Additional JVM arguments
    /// </summary>
    public List<string> CustomJvmArgs { get; set; } = new();

    /// <summary>
    /// Additional game arguments
    /// </summary>
    public List<string> CustomGameArgs { get; set; } = new();

    /// <summary>
    /// Working directory for the game
    /// </summary>
    public string? WorkingDirectory { get; set; }

    /// <summary>
    /// Whether to show the game console
    /// </summary>
    public bool ShowConsole { get; set; } = false;
}

/// <summary>
/// Launch progress information
/// </summary>
public class LaunchProgress
{
    /// <summary>
    /// Current status message
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Current step (0-100)
    /// </summary>
    public int Step { get; set; }

    /// <summary>
    /// Total steps
    /// </summary>
    public int TotalSteps { get; set; }

    /// <summary>
    /// Progress percentage
    /// </summary>
    public double Percentage => TotalSteps > 0 ? (double)Step / TotalSteps * 100 : 0;

    /// <summary>
    /// Whether launch is complete
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Whether launch failed
    /// </summary>
    public bool IsFailed { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Game process information
/// </summary>
public class GameProcess
{
    /// <summary>
    /// Process ID
    /// </summary>
    public int ProcessId { get; set; }

    /// <summary>
    /// Game instance ID
    /// </summary>
    public string GameInstanceId { get; set; } = string.Empty;

    /// <summary>
    /// Start time
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the game is still running
    /// </summary>
    public bool IsRunning { get; set; } = true;

    /// <summary>
    /// Exit code (if exited)
    /// </summary>
    public int? ExitCode { get; set; }

    /// <summary>
    /// Game log output
    /// </summary>
    public List<string> LogLines { get; set; } = new();

    /// <summary>
    /// Whether the game crashed
    /// </summary>
    public bool Crashed { get; set; }

    /// <summary>
    /// Crash reason (if crashed)
    /// </summary>
    public string? CrashReason { get; set; }
}
