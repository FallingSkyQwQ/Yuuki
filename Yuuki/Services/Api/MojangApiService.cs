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
/// Interface for Mojang API service
/// </summary>
public interface IMojangApiService
{
    /// <summary>
    /// Gets the version manifest from Mojang
    /// </summary>
    Task<List<MinecraftVersion>> GetVersionManifestAsync();

    /// <summary>
    /// Gets detailed version information
    /// </summary>
    Task<VersionDetail> GetVersionDetailAsync(string versionId, string manifestUrl);

    /// <summary>
    /// Downloads a file from Mojang CDN
    /// </summary>
    Task<byte[]> DownloadFileAsync(string url, IProgress<double>? progress = null);
}

/// <summary>
/// Implementation of Mojang API service
/// </summary>
public class MojangApiService : IMojangApiService
{
    private const string VersionManifestUrl = "https://launchermeta.mojang.com/mc/game/version_manifest.json";

    private readonly HttpClient _httpClient;
    private readonly ILogger<MojangApiService> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    public MojangApiService(HttpClient httpClient, ILogger<MojangApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Configure retry policy with exponential backoff
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Request failed. Waiting {Delay}s before retry {RetryCount}",
                        timespan.TotalSeconds, retryCount);
                });
    }

    public async Task<List<MinecraftVersion>> GetVersionManifestAsync()
    {
        try
        {
            _logger.LogInformation("Fetching Minecraft version manifest from Mojang");

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync(VersionManifestUrl));

            response.EnsureSuccessStatusCode();

            var manifest = await response.Content.ReadFromJsonAsync<VersionManifestResponse>();
            if (manifest == null)
            {
                throw new InvalidOperationException("Failed to deserialize version manifest");
            }

            var versions = manifest.Versions.Select(v => new MinecraftVersion
            {
                Id = v.Id,
                Type = v.Type,
                ReleaseTime = DateTime.TryParse(v.ReleaseTime, out var releaseTime)
                    ? releaseTime
                    : DateTime.MinValue,
                Url = v.Url,
                Sha1 = v.Sha1,
                IsInstalled = false,
                SupportedModLoaders = new List<ModLoaderCompatibility>()
            }).ToList();

            _logger.LogInformation("Retrieved {Count} Minecraft versions", versions.Count);
            return versions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Minecraft version manifest");
            throw;
        }
    }

    public async Task<VersionDetail> GetVersionDetailAsync(string versionId, string manifestUrl)
    {
        try
        {
            _logger.LogInformation("Fetching version details for {VersionId}", versionId);

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync(manifestUrl));

            response.EnsureSuccessStatusCode();

            var detail = await response.Content.ReadFromJsonAsync<VersionDetail>();
            if (detail == null)
            {
                throw new InvalidOperationException($"Failed to deserialize version detail for {versionId}");
            }

            _logger.LogInformation("Retrieved version details for {VersionId}", versionId);
            return detail;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch version details for {VersionId}", versionId);
            throw;
        }
    }

    public async Task<byte[]> DownloadFileAsync(string url, IProgress<double>? progress = null)
    {
        try
        {
            _logger.LogInformation("Downloading file from {Url}", url);

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead));

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

            _logger.LogInformation("Downloaded {Bytes} bytes from {Url}", bytesRead, url);
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file from {Url}", url);
            throw;
        }
    }
}
