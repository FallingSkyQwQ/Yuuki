using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Yuuki.Services.Api.Models;

namespace Yuuki.Services.Api;

/// <summary>
/// Interface for mod loader API service
/// </summary>
public interface IModLoaderApiService
{
    /// <summary>
    /// Gets available Fabric loader versions
    /// </summary>
    Task<List<string>> GetFabricVersionsAsync();

    /// <summary>
    /// Gets available Fabric versions for a specific game version
    /// </summary>
    Task<List<string>> GetFabricVersionsForGameAsync(string gameVersion);

    /// <summary>
    /// Gets available Forge versions for a specific game version
    /// </summary>
    Task<List<string>> GetForgeVersionsAsync(string gameVersion);

    /// <summary>
    /// Gets recommended Forge version for a game version
    /// </summary>
    Task<string?> GetRecommendedForgeVersionAsync(string gameVersion);

    /// <summary>
    /// Gets available NeoForge versions for a specific game version
    /// </summary>
    Task<List<string>> GetNeoForgeVersionsAsync(string gameVersion);
}

/// <summary>
/// Implementation of mod loader API service
/// </summary>
public class ModLoaderApiService : IModLoaderApiService
{
    private const string FabricMetaUrl = "https://meta.fabricmc.net/v2";
    private const string ForgeMetaUrl = "https://files.minecraftforge.net/net/minecraftforge/forge";
    private const string NeoForgeApiUrl = "https://maven.neoforged.net/api/maven";

    private readonly HttpClient _httpClient;
    private readonly ILogger<ModLoaderApiService> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    public ModLoaderApiService(HttpClient httpClient, ILogger<ModLoaderApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Mod loader API request failed. Waiting {Delay}s before retry {RetryCount}",
                        timespan.TotalSeconds, retryCount);
                });
    }

    public async Task<List<string>> GetFabricVersionsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching Fabric loader versions");

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync($"{FabricMetaUrl}/versions/loader"));

            response.EnsureSuccessStatusCode();

            var versions = await response.Content.ReadFromJsonAsync<List<FabricLoaderVersion>>();
            if (versions == null)
            {
                return new List<string>();
            }

            var versionList = versions.Select(v => v.Version).ToList();
            _logger.LogInformation("Retrieved {Count} Fabric loader versions", versionList.Count);
            return versionList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Fabric loader versions");
            return new List<string>();
        }
    }

    public async Task<List<string>> GetFabricVersionsForGameAsync(string gameVersion)
    {
        try
        {
            _logger.LogInformation("Fetching Fabric versions for game {GameVersion}", gameVersion);

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync($"{FabricMetaUrl}/versions/loader/{gameVersion}"));

            response.EnsureSuccessStatusCode();

            var versions = await response.Content.ReadFromJsonAsync<List<FabricLoaderVersion>>();
            if (versions == null)
            {
                return new List<string>();
            }

            var versionList = versions.Select(v => v.Version).ToList();
            _logger.LogInformation("Retrieved {Count} Fabric versions for {GameVersion}", versionList.Count, gameVersion);
            return versionList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Fabric versions for {GameVersion}", gameVersion);
            return new List<string>();
        }
    }

    public async Task<List<string>> GetForgeVersionsAsync(string gameVersion)
    {
        try
        {
            _logger.LogInformation("Fetching Forge versions for game {GameVersion}", gameVersion);

            // Note: Forge API changed over time, this is a simplified version
            // In production, you'd need to parse the promotions.json
            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync($"{ForgeMetaUrl}/promotions_slim.json"));

            response.EnsureSuccessStatusCode();

            var promos = await response.Content.ReadFromJsonAsync<ForgePromos>();
            if (promos == null)
            {
                return new List<string>();
            }

            // Filter versions for the specified game version
            var versions = promos.Promotions
                .Where(kvp => kvp.Key.StartsWith(gameVersion))
                .Select(kvp => kvp.Value)
                .Distinct()
                .ToList();

            _logger.LogInformation("Retrieved {Count} Forge versions for {GameVersion}", versions.Count, gameVersion);
            return versions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Forge versions for {GameVersion}", gameVersion);
            return new List<string>();
        }
    }

    public async Task<string?> GetRecommendedForgeVersionAsync(string gameVersion)
    {
        try
        {
            _logger.LogInformation("Fetching recommended Forge version for {GameVersion}", gameVersion);

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync($"{ForgeMetaUrl}/promotions_slim.json"));

            response.EnsureSuccessStatusCode();

            var promos = await response.Content.ReadFromJsonAsync<ForgePromos>();
            if (promos == null)
            {
                return null;
            }

            // Try to get recommended version first, then latest
            var recommendedKey = $"{gameVersion}-recommended";
            var latestKey = $"{gameVersion}-latest";

            if (promos.Promotions.TryGetValue(recommendedKey, out var recommended))
            {
                _logger.LogInformation("Found recommended Forge version {Version} for {GameVersion}",
                    recommended, gameVersion);
                return recommended;
            }

            if (promos.Promotions.TryGetValue(latestKey, out var latest))
            {
                _logger.LogInformation("Found latest Forge version {Version} for {GameVersion}",
                    latest, gameVersion);
                return latest;
            }

            _logger.LogWarning("No recommended or latest Forge version found for {GameVersion}", gameVersion);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch recommended Forge version for {GameVersion}", gameVersion);
            return null;
        }
    }

    public async Task<List<string>> GetNeoForgeVersionsAsync(string gameVersion)
    {
        try
        {
            _logger.LogInformation("Fetching NeoForge versions for game {GameVersion}", gameVersion);

            // NeoForge uses Maven metadata
            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync($"{NeoForgeApiUrl}/versions/releases/net/neoforged/neoforge"));

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            // Simple parsing - in production, parse proper Maven metadata XML
            // For now, return empty list as NeoForge API might need more complex handling
            _logger.LogInformation("NeoForge version fetching needs full Maven metadata parsing");
            return new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch NeoForge versions for {GameVersion}", gameVersion);
            return new List<string>();
        }
    }
}
