namespace Yuuki.Models;

/// <summary>
/// Types of mod loaders supported by Yuuki
/// </summary>
public enum ModLoaderType
{
    /// <summary>
    /// Fabric mod loader
    /// </summary>
    Fabric,

    /// <summary>
    /// Forge mod loader
    /// </summary>
    Forge,

    /// <summary>
    /// NeoForge mod loader (modern Forge fork)
    /// </summary>
    NeoForge
}
