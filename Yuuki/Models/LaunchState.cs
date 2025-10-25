namespace Yuuki.Models;

/// <summary>
/// Game launch states
/// </summary>
public enum LaunchState
{
    /// <summary>
    /// No active launch operation
    /// </summary>
    Idle,

    /// <summary>
    /// Preparing to launch (validating files, checking prerequisites)
    /// </summary>
    Preparing,

    /// <summary>
    /// Downloading required files
    /// </summary>
    Downloading,

    /// <summary>
    /// Installing components
    /// </summary>
    Installing,

    /// <summary>
    /// Launching the game process
    /// </summary>
    Launching,

    /// <summary>
    /// Game is currently running
    /// </summary>
    Running,

    /// <summary>
    /// An error occurred during launch
    /// </summary>
    Error
}
