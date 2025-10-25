using System;
using System.Collections.Generic;

namespace Yuuki.Models;

/// <summary>
/// Represents mod information from CurseForge or Modrinth
/// </summary>
public class ModInfo
{
    /// <summary>
    /// Unique mod ID (platform-specific)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Mod name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Short description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Mod author(s)
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// URL to mod icon/logo
    /// </summary>
    public string? IconUrl { get; set; }

    /// <summary>
    /// Supported Minecraft versions
    /// </summary>
    public List<string> SupportedVersions { get; set; } = new();

    /// <summary>
    /// Platform where this mod is hosted
    /// </summary>
    public ModPlatform Platform { get; set; }

    /// <summary>
    /// Download count
    /// </summary>
    public long DownloadCount { get; set; }

    /// <summary>
    /// Last update time
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// Mod dependencies
    /// </summary>
    public List<ModDependency> Dependencies { get; set; } = new();

    /// <summary>
    /// Project URL
    /// </summary>
    public string? ProjectUrl { get; set; }

    /// <summary>
    /// Available mod files/versions
    /// </summary>
    public List<ModFile> Files { get; set; } = new();
}

/// <summary>
/// Mod dependency information
/// </summary>
public class ModDependency
{
    /// <summary>
    /// Dependency mod ID
    /// </summary>
    public string ModId { get; set; } = string.Empty;

    /// <summary>
    /// Dependency type (required, optional, incompatible)
    /// </summary>
    public DependencyType Type { get; set; }

    /// <summary>
    /// Required version or version range
    /// </summary>
    public string? VersionRequirement { get; set; }
}

/// <summary>
/// Dependency types
/// </summary>
public enum DependencyType
{
    Required,
    Optional,
    Incompatible
}

/// <summary>
/// Mod file information
/// </summary>
public class ModFile
{
    /// <summary>
    /// File ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// File name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Display name/version
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Download URL
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>
    /// Supported Minecraft versions
    /// </summary>
    public List<string> GameVersions { get; set; } = new();

    /// <summary>
    /// Required mod loaders
    /// </summary>
    public List<ModLoaderType> RequiredLoaders { get; set; } = new();

    /// <summary>
    /// Upload date
    /// </summary>
    public DateTime UploadDate { get; set; }

    /// <summary>
    /// File hash (SHA1 or MD5)
    /// </summary>
    public string? FileHash { get; set; }
}
