using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Yuuki.Services.Api.Models;

/// <summary>
/// Response from Mojang version manifest API
/// </summary>
public class VersionManifestResponse
{
    [JsonPropertyName("latest")]
    public LatestVersions Latest { get; set; } = new();

    [JsonPropertyName("versions")]
    public List<VersionInfo> Versions { get; set; } = new();
}

/// <summary>
/// Latest version information
/// </summary>
public class LatestVersions
{
    [JsonPropertyName("release")]
    public string Release { get; set; } = string.Empty;

    [JsonPropertyName("snapshot")]
    public string Snapshot { get; set; } = string.Empty;
}

/// <summary>
/// Version information from manifest
/// </summary>
public class VersionInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;

    [JsonPropertyName("releaseTime")]
    public string ReleaseTime { get; set; } = string.Empty;

    [JsonPropertyName("sha1")]
    public string? Sha1 { get; set; }
}

/// <summary>
/// Detailed version information
/// </summary>
public class VersionDetail
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;

    [JsonPropertyName("releaseTime")]
    public string ReleaseTime { get; set; } = string.Empty;

    [JsonPropertyName("mainClass")]
    public string? MainClass { get; set; }

    [JsonPropertyName("minecraftArguments")]
    public string? MinecraftArguments { get; set; }

    [JsonPropertyName("arguments")]
    public VersionArguments? Arguments { get; set; }

    [JsonPropertyName("libraries")]
    public List<Library> Libraries { get; set; } = new();

    [JsonPropertyName("downloads")]
    public VersionDownloads Downloads { get; set; } = new();

    [JsonPropertyName("assetIndex")]
    public AssetIndex? AssetIndex { get; set; }

    [JsonPropertyName("assets")]
    public string? Assets { get; set; }

    [JsonPropertyName("javaVersion")]
    public JavaVersion? JavaVersion { get; set; }
}

/// <summary>
/// Version arguments structure
/// </summary>
public class VersionArguments
{
    [JsonPropertyName("game")]
    public List<object> Game { get; set; } = new();

    [JsonPropertyName("jvm")]
    public List<object> Jvm { get; set; } = new();
}

/// <summary>
/// Library information
/// </summary>
public class Library
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("downloads")]
    public LibraryDownloads? Downloads { get; set; }

    [JsonPropertyName("rules")]
    public List<Rule>? Rules { get; set; }

    [JsonPropertyName("natives")]
    public Dictionary<string, string>? Natives { get; set; }

    [JsonPropertyName("extract")]
    public ExtractRule? Extract { get; set; }
}

/// <summary>
/// Library download information
/// </summary>
public class LibraryDownloads
{
    [JsonPropertyName("artifact")]
    public Artifact? Artifact { get; set; }

    [JsonPropertyName("classifiers")]
    public Dictionary<string, Artifact>? Classifiers { get; set; }
}

/// <summary>
/// Download artifact
/// </summary>
public class Artifact
{
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("sha1")]
    public string? Sha1 { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// Rule for conditional library loading
/// </summary>
public class Rule
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    [JsonPropertyName("os")]
    public OsRule? Os { get; set; }
}

/// <summary>
/// OS-specific rule
/// </summary>
public class OsRule
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("arch")]
    public string? Arch { get; set; }
}

/// <summary>
/// Extract rule for native libraries
/// </summary>
public class ExtractRule
{
    [JsonPropertyName("exclude")]
    public List<string>? Exclude { get; set; }
}

/// <summary>
/// Version downloads
/// </summary>
public class VersionDownloads
{
    [JsonPropertyName("client")]
    public Artifact? Client { get; set; }

    [JsonPropertyName("server")]
    public Artifact? Server { get; set; }

    [JsonPropertyName("client_mappings")]
    public Artifact? ClientMappings { get; set; }

    [JsonPropertyName("server_mappings")]
    public Artifact? ServerMappings { get; set; }
}

/// <summary>
/// Asset index information
/// </summary>
public class AssetIndex
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("sha1")]
    public string? Sha1 { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("totalSize")]
    public long TotalSize { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// Java version requirement
/// </summary>
public class JavaVersion
{
    [JsonPropertyName("component")]
    public string Component { get; set; } = string.Empty;

    [JsonPropertyName("majorVersion")]
    public int MajorVersion { get; set; }
}
