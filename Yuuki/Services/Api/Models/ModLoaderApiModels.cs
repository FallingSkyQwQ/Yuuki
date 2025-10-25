using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Yuuki.Services.Api.Models;

/// <summary>
/// Fabric Meta API response for loader versions
/// </summary>
public class FabricLoaderVersion
{
    [JsonPropertyName("separator")]
    public string? Separator { get; set; }

    [JsonPropertyName("build")]
    public int Build { get; set; }

    [JsonPropertyName("maven")]
    public string Maven { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("stable")]
    public bool Stable { get; set; }
}

/// <summary>
/// Fabric game version
/// </summary>
public class FabricGameVersion
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("stable")]
    public bool Stable { get; set; }
}

/// <summary>
/// Forge version manifest
/// </summary>
public class ForgeVersionManifest
{
    [JsonPropertyName("gameVersion")]
    public string? GameVersion { get; set; }

    [JsonPropertyName("latest")]
    public string? Latest { get; set; }

    [JsonPropertyName("recommended")]
    public string? Recommended { get; set; }

    [JsonPropertyName("versions")]
    public List<string>? Versions { get; set; }
}

/// <summary>
/// Forge promotion information
/// </summary>
public class ForgePromos
{
    [JsonPropertyName("homepage")]
    public string? Homepage { get; set; }

    [JsonPropertyName("promos")]
    public Dictionary<string, string> Promotions { get; set; } = new();
}
