using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Yuuki.Models;
using Yuuki.Services.Api.Models;

namespace Yuuki.Services.Api;

/// <summary>
/// Interface for Modrinth API service
/// </summary>
public interface IModrinthApiService
{
    /// <summary>
    /// Searches for mods on Modrinth
    /// </summary>
    Task<List<ModInfo>> SearchModsAsync(string query, string? gameVersion = null, int limit = 20, int offset = 0);

    /// <summary>
    /// Gets project details by ID or slug
    /// </summary>
    Task<ModInfo?> GetProjectAsync(string idOrSlug);

    /// <summary>
    /// Gets versions for a project
    /// </summary>
    Task<List<ModFile>> GetProjectVersionsAsync(string projectId, string? gameVersion = null);

    /// <summary>
    /// Downloads a mod file
    /// </summary>
    Task<byte[]> DownloadModAsync(string downloadUrl, IProgress<double>? progress = null);
}

/// <summary>
/// Implementation of Modrinth API service
/// </summary>
public class ModrinthApiService : IModrinthApiService
{
    private const string BaseUrl = "https://api.modrinth.com/v2";
    private const string UserAgent = "FallingSkyQwQ/Yuuki/1.0.0 (charlie.0111@foxmail.com)";

    private readonly HttpClient _httpClient;
    private readonly ILogger<ModrinthApiService> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    public ModrinthApiService(HttpClient httpClient, ILogger<ModrinthApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Set user agent as required by Modrinth API
        _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);

        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Modrinth API request failed. Waiting {Delay}s before retry {RetryCount}",
                        timespan.TotalSeconds, retryCount);
                });
    }

    public async Task<List<ModInfo>> SearchModsAsync(string query, string? gameVersion = null, int limit = 20, int offset = 0)
    {
        try
        {
            _logger.LogInformation("Searching Modrinth for '{Query}' (version: {Version})", query, gameVersion ?? "any");

            var facets = new List<string>();
            if (!string.IsNullOrEmpty(gameVersion))
            {
                facets.Add($"[[\"versions:{gameVersion}\"]]");
            }

            var facetsParam = facets.Count > 0 ? $"&facets={string.Join(",", facets)}" : "";
            var url = $"{BaseUrl}/search?query={Uri.EscapeDataString(query)}&limit={limit}&offset={offset}{facetsParam}";

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync(url));

            response.EnsureSuccessStatusCode();

            var searchResult = await response.Content.ReadFromJsonAsync<ModrinthSearchResponse>();
            if (searchResult == null)
            {
                return new List<ModInfo>();
            }

            var mods = searchResult.Hits.Select(hit => new ModInfo
            {
                Id = hit.ProjectId,
                Name = hit.Title,
                Description = hit.Description,
                Author = hit.Author,
                IconUrl = hit.IconUrl,
                SupportedVersions = hit.Versions,
                Platform = ModPlatform.Modrinth,
                DownloadCount = hit.Downloads,
                LastUpdated = DateTime.TryParse(hit.DateModified, out var modified)
                    ? modified
                    : DateTime.MinValue,
                ProjectUrl = $"https://modrinth.com/mod/{hit.Slug}",
                Files = new List<ModFile>()
            }).ToList();

            _logger.LogInformation("Found {Count} mods on Modrinth for '{Query}'", mods.Count, query);
            return mods;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search Modrinth for '{Query}'", query);
            return new List<ModInfo>();
        }
    }

    public async Task<ModInfo?> GetProjectAsync(string idOrSlug)
    {
        try
        {
            _logger.LogInformation("Fetching Modrinth project {IdOrSlug}", idOrSlug);

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync($"{BaseUrl}/project/{idOrSlug}"));

            response.EnsureSuccessStatusCode();

            var project = await response.Content.ReadFromJsonAsync<ModrinthProject>();
            if (project == null)
            {
                return null;
            }

            var modInfo = new ModInfo
            {
                Id = project.ProjectId,
                Name = project.Title,
                Description = project.Description,
                Author = project.Author,
                IconUrl = project.IconUrl,
                SupportedVersions = project.Versions,
                Platform = ModPlatform.Modrinth,
                DownloadCount = project.Downloads,
                LastUpdated = DateTime.TryParse(project.DateModified, out var modified)
                    ? modified
                    : DateTime.MinValue,
                ProjectUrl = $"https://modrinth.com/mod/{project.Slug}",
                Files = new List<ModFile>()
            };

            _logger.LogInformation("Retrieved Modrinth project {ProjectId}", project.ProjectId);
            return modInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Modrinth project {IdOrSlug}", idOrSlug);
            return null;
        }
    }

    public async Task<List<ModFile>> GetProjectVersionsAsync(string projectId, string? gameVersion = null)
    {
        try
        {
            _logger.LogInformation("Fetching versions for Modrinth project {ProjectId}", projectId);

            var url = $"{BaseUrl}/project/{projectId}/version";
            if (!string.IsNullOrEmpty(gameVersion))
            {
                url += $"?game_versions=[\"{gameVersion}\"]";
            }

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync(url));

            response.EnsureSuccessStatusCode();

            var versions = await response.Content.ReadFromJsonAsync<List<ModrinthVersion>>();
            if (versions == null)
            {
                return new List<ModFile>();
            }

            var files = versions.SelectMany(version =>
                version.Files.Where(f => f.Primary).Select(file => new ModFile
                {
                    Id = version.Id,
                    FileName = file.Filename,
                    DisplayName = version.Name,
                    FileSize = file.Size,
                    DownloadUrl = file.Url,
                    GameVersions = version.GameVersions,
                    RequiredLoaders = ParseLoaders(version.Loaders),
                    UploadDate = DateTime.TryParse(version.DatePublished, out var published)
                        ? published
                        : DateTime.MinValue,
                    FileHash = file.Hashes?.Sha1
                })).ToList();

            _logger.LogInformation("Retrieved {Count} versions for project {ProjectId}", files.Count, projectId);
            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch versions for Modrinth project {ProjectId}", projectId);
            return new List<ModFile>();
        }
    }

    public async Task<byte[]> DownloadModAsync(string downloadUrl, IProgress<double>? progress = null)
    {
        try
        {
            _logger.LogInformation("Downloading mod from Modrinth: {Url}", downloadUrl);

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead));

            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            var buffer = new byte[8192];
            var bytesRead = 0L;

            using var stream = await response.Content.ReadAsStreamAsync();
            using var memoryStream = new System.IO.MemoryStream();

            int read;
            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await memoryStream.WriteAsync(buffer, 0, read);
                bytesRead += read;

                if (progress != null && totalBytes > 0)
                {
                    progress.Report((double)bytesRead / totalBytes * 100);
                }
            }

            _logger.LogInformation("Downloaded {Bytes} bytes from Modrinth", bytesRead);
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download mod from Modrinth: {Url}", downloadUrl);
            throw;
        }
    }

    private static List<ModLoaderType> ParseLoaders(List<string> loaders)
    {
        var result = new List<ModLoaderType>();

        foreach (var loader in loaders)
        {
            if (Enum.TryParse<ModLoaderType>(loader, true, out var loaderType))
            {
                result.Add(loaderType);
            }
        }

        return result;
    }
}
