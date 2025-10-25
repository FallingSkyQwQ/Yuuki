using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Yuuki.Exceptions;
using Yuuki.Models;
using Yuuki.Services.Api;
using Yuuki.Services.Api.Models;

namespace Yuuki.Services.Version;

/// <summary>
/// Interface for version manager service
/// </summary>
public interface IVersionManager
{
    /// <summary>
    /// Gets available Minecraft versions
    /// </summary>
    Task<List<MinecraftVersion>> GetAvailableVersionsAsync();

    /// <summary>
    /// Downloads and installs a Minecraft version
    /// </summary>
    Task<DownloadResult> DownloadVersionAsync(
        string versionId,
        ModLoaderType? modLoader = null,
        string? modLoaderVersion = null,
        IProgress<DownloadProgress>? progress = null);

    /// <summary>
    /// Deletes a version
    /// </summary>
    Task<bool> DeleteVersionAsync(string versionId);

    /// <summary>
    /// Validates version files integrity
    /// </summary>
    Task<bool> ValidateVersionAsync(string versionId);

    /// <summary>
    /// Gets installed versions
    /// </summary>
    Task<List<string>> GetInstalledVersionsAsync();
}

/// <summary>
/// Implementation of version manager
/// </summary>
public class VersionManager : IVersionManager
{
    private readonly IMojangApiService _mojangApi;
    private readonly IModLoaderApiService _modLoaderApi;
    private readonly ILogger<VersionManager> _logger;
    private readonly string _versionsPath;
    private readonly string _librariesPath;
    private readonly string _assetsPath;

    public VersionManager(
        IMojangApiService mojangApi,
        IModLoaderApiService modLoaderApi,
        ILogger<VersionManager> logger)
    {
        _mojangApi = mojangApi;
        _modLoaderApi = modLoaderApi;
        _logger = logger;

        var basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Yuuki",
            "minecraft");

        _versionsPath = Path.Combine(basePath, "versions");
        _librariesPath = Path.Combine(basePath, "libraries");
        _assetsPath = Path.Combine(basePath, "assets");

        // Ensure directories exist
        Directory.CreateDirectory(_versionsPath);
        Directory.CreateDirectory(_librariesPath);
        Directory.CreateDirectory(_assetsPath);
    }

    public async Task<List<MinecraftVersion>> GetAvailableVersionsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching available Minecraft versions");

            var versions = await _mojangApi.GetVersionManifestAsync();
            var installedVersions = await GetInstalledVersionsAsync();

            // Mark installed versions
            foreach (var version in versions)
            {
                version.IsInstalled = installedVersions.Contains(version.Id);
            }

            _logger.LogInformation("Retrieved {Count} versions, {Installed} installed",
                versions.Count, installedVersions.Count);

            return versions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available versions");
            throw;
        }
    }

    public async Task<DownloadResult> DownloadVersionAsync(
        string versionId,
        ModLoaderType? modLoader = null,
        string? modLoaderVersion = null,
        IProgress<DownloadProgress>? progress = null)
    {
        try
        {
            _logger.LogInformation("Starting download for version {VersionId} with loader {Loader}",
                versionId, modLoader?.ToString() ?? "None");

            var downloadProgress = new DownloadProgress
            {
                Status = "Fetching version manifest..."
            };
            progress?.Report(downloadProgress);

            // Get version manifest
            var versions = await _mojangApi.GetVersionManifestAsync();
            var versionInfo = versions.FirstOrDefault(v => v.Id == versionId);
            if (versionInfo == null)
            {
                throw new InvalidOperationException($"Version {versionId} not found");
            }

            // Get version details
            var versionDetail = await _mojangApi.GetVersionDetailAsync(versionId, versionInfo.Url);

            // Create version directory
            var versionDir = Path.Combine(_versionsPath, versionId);
            Directory.CreateDirectory(versionDir);

            downloadProgress.Status = "Downloading client JAR...";
            progress?.Report(downloadProgress);

            // Download client JAR
            if (versionDetail.Downloads?.Client != null)
            {
                var clientJar = await _mojangApi.DownloadFileAsync(
                    versionDetail.Downloads.Client.Url,
                    new Progress<double>(p =>
                    {
                        downloadProgress.DownloadedBytes = (long)(versionDetail.Downloads.Client.Size * p / 100);
                        downloadProgress.TotalBytes = versionDetail.Downloads.Client.Size;
                        downloadProgress.CurrentFile = "client.jar";
                        progress?.Report(downloadProgress);
                    }));

                var clientPath = Path.Combine(versionDir, $"{versionId}.jar");
                await File.WriteAllBytesAsync(clientPath, clientJar);

                // Verify hash
                if (!string.IsNullOrEmpty(versionDetail.Downloads.Client.Sha1))
                {
                    var hash = ComputeSha1(clientJar);
                    if (!hash.Equals(versionDetail.Downloads.Client.Sha1, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new DownloadException("Client JAR hash mismatch");
                    }
                }
            }

            downloadProgress.Status = "Downloading libraries...";
            downloadProgress.DownloadedBytes = versionDetail.Downloads?.Client?.Size ?? 0;
            progress?.Report(downloadProgress);

            // Download libraries
            var libraryCount = versionDetail.Libraries.Count;
            var downloadedLibs = 0;

            foreach (var library in versionDetail.Libraries)
            {
                if (ShouldDownloadLibrary(library))
                {
                    await DownloadLibraryAsync(library);
                }

                downloadedLibs++;
                downloadProgress.CurrentFile = library.Name;
                progress?.Report(downloadProgress);
            }

            downloadProgress.Status = "Downloading assets...";
            progress?.Report(downloadProgress);

            // Download assets index
            if (versionDetail.AssetIndex != null)
            {
                await DownloadAssetsAsync(versionDetail.AssetIndex, progress);
            }

            // Download mod loader if specified
            if (modLoader.HasValue && !string.IsNullOrEmpty(modLoaderVersion))
            {
                downloadProgress.Status = $"Installing {modLoader.Value} loader...";
                progress?.Report(downloadProgress);

                await InstallModLoaderAsync(versionId, modLoader.Value, modLoaderVersion);
            }

            // Save version JSON
            var versionJsonPath = Path.Combine(versionDir, $"{versionId}.json");
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            var versionJson = JsonSerializer.Serialize(versionDetail, jsonOptions);
            await File.WriteAllTextAsync(versionJsonPath, versionJson);

            downloadProgress.Status = "Download complete!";
            downloadProgress.IsComplete = true;
            progress?.Report(downloadProgress);

            _logger.LogInformation("Successfully downloaded version {VersionId}", versionId);

            return DownloadResult.Successful(versionDir, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download version {VersionId}", versionId);

            var failProgress = new DownloadProgress
            {
                IsFailed = true,
                ErrorMessage = ex.Message
            };
            progress?.Report(failProgress);

            return DownloadResult.Failed(ex.Message);
        }
    }

    public Task<bool> DeleteVersionAsync(string versionId)
    {
        try
        {
            _logger.LogInformation("Deleting version {VersionId}", versionId);

            var versionDir = Path.Combine(_versionsPath, versionId);
            if (Directory.Exists(versionDir))
            {
                Directory.Delete(versionDir, recursive: true);
                _logger.LogInformation("Deleted version {VersionId}", versionId);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete version {VersionId}", versionId);
            return Task.FromResult(false);
        }
    }

    public Task<bool> ValidateVersionAsync(string versionId)
    {
        try
        {
            _logger.LogInformation("Validating version {VersionId}", versionId);

            var versionDir = Path.Combine(_versionsPath, versionId);
            var versionJsonPath = Path.Combine(versionDir, $"{versionId}.json");
            var clientJarPath = Path.Combine(versionDir, $"{versionId}.jar");

            if (!File.Exists(versionJsonPath) || !File.Exists(clientJarPath))
            {
                return Task.FromResult(false);
            }

            // Could add more validation here (hash checks, etc.)
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate version {VersionId}", versionId);
            return Task.FromResult(false);
        }
    }

    public Task<List<string>> GetInstalledVersionsAsync()
    {
        try
        {
            if (!Directory.Exists(_versionsPath))
            {
                return Task.FromResult(new List<string>());
            }

            var versions = Directory.GetDirectories(_versionsPath)
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrEmpty(name))
                .Cast<string>()
                .ToList();

            return Task.FromResult(versions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get installed versions");
            return Task.FromResult(new List<string>());
        }
    }

    private bool ShouldDownloadLibrary(Library library)
    {
        // Check OS rules
        if (library.Rules != null && library.Rules.Count > 0)
        {
            foreach (var rule in library.Rules)
            {
                var allowed = rule.Action == "allow";
                if (rule.Os != null)
                {
                    // Simple OS check - could be improved
                    var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
                    if (rule.Os.Name == "windows" && !isWindows) return !allowed;
                    if (rule.Os.Name != "windows" && isWindows) return !allowed;
                }
            }
        }

        return true;
    }

    private async Task DownloadLibraryAsync(Library library)
    {
        if (library.Downloads?.Artifact == null) return;

        var artifact = library.Downloads.Artifact;
        var libraryPath = Path.Combine(_librariesPath, artifact.Path ?? "");
        var libraryDir = Path.GetDirectoryName(libraryPath);

        if (string.IsNullOrEmpty(libraryDir)) return;
        if (File.Exists(libraryPath)) return; // Already downloaded

        Directory.CreateDirectory(libraryDir);

        var fileData = await _mojangApi.DownloadFileAsync(artifact.Url);
        await File.WriteAllBytesAsync(libraryPath, fileData);
    }

    private async Task DownloadAssetsAsync(AssetIndex assetIndex, IProgress<DownloadProgress>? progress)
    {
        // Download asset index JSON
        var indexPath = Path.Combine(_assetsPath, "indexes", $"{assetIndex.Id}.json");
        var indexDir = Path.GetDirectoryName(indexPath);
        if (!string.IsNullOrEmpty(indexDir))
        {
            Directory.CreateDirectory(indexDir);
        }

        if (!File.Exists(indexPath))
        {
            var indexData = await _mojangApi.DownloadFileAsync(assetIndex.Url);
            await File.WriteAllBytesAsync(indexPath, indexData);
        }

        // Note: Downloading individual assets would be done here
        // For now we just download the index
    }

    private async Task InstallModLoaderAsync(string versionId, ModLoaderType loaderType, string loaderVersion)
    {
        _logger.LogInformation("Installing {Loader} {Version} for {GameVersion}",
            loaderType, loaderVersion, versionId);

        // Mod loader installation would be implemented here
        // This is a stub for now
        await Task.CompletedTask;
    }

    private static string ComputeSha1(byte[] data)
    {
        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
