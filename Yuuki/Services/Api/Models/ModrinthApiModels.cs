using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Yuuki.Services.Api.Models;

/// <summary>
/// Modrinth search response
/// </summary>
public class ModrinthSearchResponse
{
    [JsonPropertyName("hits")]
    public List<ModrinthProject> Hits { get; set; } = new();

    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("total_hits")]
    public int TotalHits { get; set; }
}

/// <summary>
/// Modrinth project information
/// </summary>
public class ModrinthProject
{
    [JsonPropertyName("project_id")]
    public string ProjectId { get; set; } = string.Empty;

    [JsonPropertyName("project_type")]
    public string ProjectType { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("categories")]
    public List<string> Categories { get; set; } = new();

    [JsonPropertyName("client_side")]
    public string ClientSide { get; set; } = string.Empty;

    [JsonPropertyName("server_side")]
    public string ServerSide { get; set; } = string.Empty;

    [JsonPropertyName("downloads")]
    public long Downloads { get; set; }

    [JsonPropertyName("follows")]
    public long Follows { get; set; }

    [JsonPropertyName("icon_url")]
    public string? IconUrl { get; set; }

    [JsonPropertyName("date_created")]
    public string DateCreated { get; set; } = string.Empty;

    [JsonPropertyName("date_modified")]
    public string DateModified { get; set; } = string.Empty;

    [JsonPropertyName("latest_version")]
    public string? LatestVersion { get; set; }

    [JsonPropertyName("versions")]
    public List<string> Versions { get; set; } = new();

    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("gallery")]
    public List<string>? Gallery { get; set; }
}

/// <summary>
/// Modrinth project version
/// </summary>
public class ModrinthVersion
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("project_id")]
    public string ProjectId { get; set; } = string.Empty;

    [JsonPropertyName("author_id")]
    public string AuthorId { get; set; } = string.Empty;

    [JsonPropertyName("featured")]
    public bool Featured { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version_number")]
    public string VersionNumber { get; set; } = string.Empty;

    [JsonPropertyName("changelog")]
    public string? Changelog { get; set; }

    [JsonPropertyName("date_published")]
    public string DatePublished { get; set; } = string.Empty;

    [JsonPropertyName("downloads")]
    public long Downloads { get; set; }

    [JsonPropertyName("version_type")]
    public string VersionType { get; set; } = string.Empty;

    [JsonPropertyName("files")]
    public List<ModrinthFile> Files { get; set; } = new();

    [JsonPropertyName("dependencies")]
    public List<ModrinthDependency> Dependencies { get; set; } = new();

    [JsonPropertyName("game_versions")]
    public List<string> GameVersions { get; set; } = new();

    [JsonPropertyName("loaders")]
    public List<string> Loaders { get; set; } = new();
}

/// <summary>
/// Modrinth file information
/// </summary>
public class ModrinthFile
{
    [JsonPropertyName("hashes")]
    public ModrinthFileHashes? Hashes { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    [JsonPropertyName("primary")]
    public bool Primary { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("file_type")]
    public string? FileType { get; set; }
}

/// <summary>
/// File hashes
/// </summary>
public class ModrinthFileHashes
{
    [JsonPropertyName("sha1")]
    public string? Sha1 { get; set; }

    [JsonPropertyName("sha512")]
    public string? Sha512 { get; set; }
}

/// <summary>
/// Modrinth dependency
/// </summary>
public class ModrinthDependency
{
    [JsonPropertyName("version_id")]
    public string? VersionId { get; set; }

    [JsonPropertyName("project_id")]
    public string? ProjectId { get; set; }

    [JsonPropertyName("file_name")]
    public string? FileName { get; set; }

    [JsonPropertyName("dependency_type")]
    public string DependencyType { get; set; } = string.Empty;
}
