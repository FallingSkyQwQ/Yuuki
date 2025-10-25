using System;
using System.Collections.Generic;

namespace Yuuki.Models;

/// <summary>
/// Represents a Minecraft version from the version manifest
/// </summary>
public class MinecraftVersion
{
    /// <summary>
    /// Version identifier (e.g., "1.20.1")
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Version type: release, snapshot, old_beta, old_alpha
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// When this version was released
    /// </summary>
    public DateTime ReleaseTime { get; set; }

    /// <summary>
    /// URL to the version manifest JSON
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// SHA1 hash of the version manifest
    /// </summary>
    public string? Sha1 { get; set; }

    /// <summary>
    /// Whether this version is installed locally
    /// </summary>
    public bool IsInstalled { get; set; }

    /// <summary>
    /// Supported mod loaders for this version
    /// </summary>
    public List<ModLoaderCompatibility> SupportedModLoaders { get; set; } = new();
}

/// <summary>
/// Mod loader compatibility information
/// </summary>
public class ModLoaderCompatibility
{
    /// <summary>
    /// Type of mod loader
    /// </summary>
    public ModLoaderType LoaderType { get; set; }

    /// <summary>
    /// Available versions for this mod loader
    /// </summary>
    public List<string> AvailableVersions { get; set; } = new();

    /// <summary>
    /// Recommended version
    /// </summary>
    public string? RecommendedVersion { get; set; }
}
