using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Yuuki.Data.Repositories;
using Yuuki.Exceptions;
using Yuuki.Models;
using Yuuki.Services.Api;
using Yuuki.Services.Api.Models;

namespace Yuuki.Services.Mod;

/// <summary>
/// Interface for mod management service
/// </summary>
public interface IModManager
{
    /// <summary>
    /// Searches for mods across platforms
    /// </summary>
    Task<List<ModInfo>> SearchModsAsync(
        string query,
        ModPlatform platform,
        string? minecraftVersion = null,
        ModLoaderType? modLoader = null,
        int limit = 20);

    /// <summary>
    /// Downloads and installs a mod to a game instance
    /// </summary>
    Task<InstalledMod> InstallModAsync(
        string gameInstanceId,
        string modId,
        string versionId,
        ModPlatform platform,
        IProgress<DownloadProgress>? progress = null);

    /// <summary>
    /// Uninstalls a mod from a game instance
    /// </summary>
    Task<bool> UninstallModAsync(string installedModId);

    /// <summary>
    /// Enables or disables a mod
    /// </summary>
    Task<bool> ToggleModAsync(string installedModId, bool enabled);

    /// <summary>
    /// Checks for mod updates
    /// </summary>
    Task<List<ModUpdateInfo>> CheckForUpdatesAsync(string gameInstanceId);

    /// <summary>
    /// Updates a mod to the latest compatible version
    /// </summary>
    Task<InstalledMod> UpdateModAsync(
        string installedModId,
        IProgress<DownloadProgress>? progress = null);

    /// <summary>
    /// Gets mod dependencies and checks compatibility
    /// </summary>
    Task<ModCompatibilityResult> CheckCompatibilityAsync(
        string gameInstanceId,
        string modId,
        ModPlatform platform);

    /// <summary>
    /// Gets installed mods for a game instance
    /// </summary>
    Task<List<InstalledMod>> GetInstalledModsAsync(string gameInstanceId);
}

/// <summary>
/// Mod update information
/// </summary>
public class ModUpdateInfo
{
    public string InstalledModId { get; set; } = string.Empty;
    public string ModName { get; set; } = string.Empty;
    public string CurrentVersion { get; set; } = string.Empty;
    public string LatestVersion { get; set; } = string.Empty;
    public string LatestVersionId { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public string Changelog { get; set; } = string.Empty;
}

/// <summary>
/// Mod compatibility check result
/// </summary>
public class ModCompatibilityResult
{
    public bool IsCompatible { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<ModrinthDependency> MissingDependencies { get; set; } = new();
    public List<string> Conflicts { get; set; } = new();
}

/// <summary>
/// Implementation of mod manager
/// </summary>
public class ModManager : IModManager
{
    private readonly IModrinthApiService _modrinthApi;
    private readonly IInstalledModRepository _modRepository;
    private readonly IGameInstanceRepository _instanceRepository;
    private readonly ILogger<ModManager> _logger;

    public ModManager(
        IModrinthApiService modrinthApi,
        IInstalledModRepository modRepository,
        IGameInstanceRepository instanceRepository,
        ILogger<ModManager> logger)
    {
        _modrinthApi = modrinthApi;
        _modRepository = modRepository;
        _instanceRepository = instanceRepository;
        _logger = logger;
    }

    public async Task<List<ModInfo>> SearchModsAsync(
        string query,
        ModPlatform platform,
        string? minecraftVersion = null,
        ModLoaderType? modLoader = null,
        int limit = 20)
    {
        try
        {
            _logger.LogInformation("Searching mods: {Query} on {Platform}", query, platform);

            // Currently only Modrinth is implemented
            if (platform != ModPlatform.Modrinth)
            {
                throw new NotImplementedException($"Platform {platform} is not yet implemented");
            }

            var mods = await _modrinthApi.SearchModsAsync(query, minecraftVersion, limit);

            _logger.LogInformation("Found {Count} mods", mods.Count);
            return mods;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search mods");
            throw;
        }
    }

    public async Task<InstalledMod> InstallModAsync(
        string gameInstanceId,
        string modId,
        string versionId,
        ModPlatform platform,
        IProgress<DownloadProgress>? progress = null)
    {
        try
        {
            _logger.LogInformation("Installing mod {ModId} version {VersionId} to instance {InstanceId}",
                modId, versionId, gameInstanceId);

            var instance = await _instanceRepository.GetByIdAsync(gameInstanceId);
            if (instance == null)
            {
                throw new InvalidOperationException($"Game instance {gameInstanceId} not found");
            }

            var downloadProgress = new DownloadProgress
            {
                Status = "Fetching mod information..."
            };
            progress?.Report(downloadProgress);

            // Get mod version details
            if (platform != ModPlatform.Modrinth)
            {
                throw new NotImplementedException($"Platform {platform} is not yet implemented");
            }

            var modVersions = await _modrinthApi.GetProjectVersionsAsync(modId);
            var selectedVersion = modVersions.FirstOrDefault(v => v.Id == versionId);
            if (selectedVersion == null)
            {
                throw new InvalidOperationException($"Mod version {versionId} not found");
            }

            downloadProgress.Status = $"Downloading {selectedVersion.FileName}...";
            progress?.Report(downloadProgress);

            // Determine mods directory
            var modsDir = GetModsDirectory(instance);
            Directory.CreateDirectory(modsDir);

            var filePath = Path.Combine(modsDir, selectedVersion.FileName);

            // Download mod file
            var fileData = await _modrinthApi.DownloadModAsync(
                selectedVersion.DownloadUrl,
                new Progress<double>(p =>
                {
                    downloadProgress.DownloadedBytes = (long)(selectedVersion.FileSize * p / 100);
                    downloadProgress.TotalBytes = selectedVersion.FileSize;
                    downloadProgress.CurrentFile = selectedVersion.FileName;
                    progress?.Report(downloadProgress);
                }));

            await File.WriteAllBytesAsync(filePath, fileData);

            // Verify file hash if available
            if (!string.IsNullOrEmpty(selectedVersion.FileHash))
            {
                var hash = ComputeSha1(fileData);
                if (!hash.Equals(selectedVersion.FileHash, StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(filePath);
                    throw new DownloadException("Mod file hash mismatch");
                }
            }

            // Get mod project details for metadata
            var project = await _modrinthApi.GetProjectAsync(modId);
            if (project == null)
            {
                throw new InvalidOperationException($"Project {modId} not found");
            }

            // Create installed mod record
            var installedMod = new InstalledMod
            {
                GameInstanceId = gameInstanceId,
                ModId = modId,
                Name = project.Name,
                Version = selectedVersion.DisplayName,
                FileName = selectedVersion.FileName,
                Platform = platform,
                IsEnabled = true,
                InstalledAt = DateTime.UtcNow,
                LatestVersion = selectedVersion.DisplayName
            };

            await _modRepository.AddAsync(installedMod);

            downloadProgress.Status = "Installation complete!";
            downloadProgress.IsComplete = true;
            progress?.Report(downloadProgress);

            _logger.LogInformation("Successfully installed mod {ModName} version {Version}",
                installedMod.Name, installedMod.Version);

            return installedMod;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to install mod {ModId}", modId);

            var failProgress = new DownloadProgress
            {
                IsFailed = true,
                ErrorMessage = ex.Message
            };
            progress?.Report(failProgress);

            throw;
        }
    }

    public async Task<bool> UninstallModAsync(string installedModId)
    {
        try
        {
            _logger.LogInformation("Uninstalling mod {ModId}", installedModId);

            var mod = await _modRepository.GetByIdAsync(installedModId);
            if (mod == null)
            {
                return false;
            }

            // Determine mods directory from instance
            var instance = await _instanceRepository.GetByIdAsync(mod.GameInstanceId);
            if (instance != null)
            {
                var modsDir = GetModsDirectory(instance);
                var filePath = Path.Combine(modsDir, mod.FileName);

                // Delete mod file
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Also check for .disabled file
                var disabledPath = filePath + ".disabled";
                if (File.Exists(disabledPath))
                {
                    File.Delete(disabledPath);
                }
            }

            // Remove from database
            await _modRepository.DeleteAsync(mod);

            _logger.LogInformation("Successfully uninstalled mod {ModName}", mod.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to uninstall mod {ModId}", installedModId);
            return false;
        }
    }

    public async Task<bool> ToggleModAsync(string installedModId, bool enabled)
    {
        try
        {
            _logger.LogInformation("{Action} mod {ModId}",
                enabled ? "Enabling" : "Disabling", installedModId);

            var mod = await _modRepository.GetByIdAsync(installedModId);
            if (mod == null)
            {
                return false;
            }

            var instance = await _instanceRepository.GetByIdAsync(mod.GameInstanceId);
            if (instance == null)
            {
                return false;
            }

            var modsDir = GetModsDirectory(instance);
            var currentPath = Path.Combine(modsDir, mod.FileName);
            var disabledPath = currentPath + ".disabled";

            // Rename file to disable/enable
            if (enabled)
            {
                // Enable: rename from .disabled to normal
                if (File.Exists(disabledPath))
                {
                    File.Move(disabledPath, currentPath);
                }
            }
            else
            {
                // Disable: rename to .disabled
                if (File.Exists(currentPath))
                {
                    File.Move(currentPath, disabledPath);
                }
            }

            mod.IsEnabled = enabled;
            await _modRepository.UpdateAsync(mod);

            _logger.LogInformation("Successfully {Action} mod {ModName}",
                enabled ? "enabled" : "disabled", mod.Name);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle mod {ModId}", installedModId);
            return false;
        }
    }

    public async Task<List<ModUpdateInfo>> CheckForUpdatesAsync(string gameInstanceId)
    {
        try
        {
            _logger.LogInformation("Checking for mod updates for instance {InstanceId}", gameInstanceId);

            var instance = await _instanceRepository.GetByIdAsync(gameInstanceId);
            if (instance == null)
            {
                throw new InvalidOperationException($"Game instance {gameInstanceId} not found");
            }

            var installedMods = await _modRepository.GetByGameInstanceAsync(gameInstanceId);
            var updates = new List<ModUpdateInfo>();

            foreach (var mod in installedMods)
            {
                try
                {
                    if (mod.Platform != ModPlatform.Modrinth)
                    {
                        continue; // Skip non-Modrinth mods for now
                    }

                    var versions = await _modrinthApi.GetProjectVersionsAsync(mod.ModId, instance.MinecraftVersion);

                    // Find latest compatible version
                    var latestVersion = versions
                        .Where(v => v.GameVersions.Contains(instance.MinecraftVersion))
                        .OrderByDescending(v => DateTime.TryParse(v.UploadDate.ToString(), out var date) ? date : DateTime.MinValue)
                        .FirstOrDefault();

                    if (latestVersion != null && latestVersion.DisplayName != mod.Version)
                    {
                        updates.Add(new ModUpdateInfo
                        {
                            InstalledModId = mod.Id,
                            ModName = mod.Name,
                            CurrentVersion = mod.Version,
                            LatestVersion = latestVersion.DisplayName,
                            LatestVersionId = latestVersion.Id,
                            ReleaseDate = latestVersion.UploadDate,
                            Changelog = ""
                        });

                        // Update latest version in database
                        mod.LatestVersion = latestVersion.DisplayName;
                        mod.HasUpdate = true;
                        await _modRepository.UpdateAsync(mod);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to check updates for mod {ModName}", mod.Name);
                }
            }

            _logger.LogInformation("Found {Count} mod updates", updates.Count);
            return updates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check for mod updates");
            throw;
        }
    }

    public async Task<InstalledMod> UpdateModAsync(
        string installedModId,
        IProgress<DownloadProgress>? progress = null)
    {
        try
        {
            _logger.LogInformation("Updating mod {ModId}", installedModId);

            var mod = await _modRepository.GetByIdAsync(installedModId);
            if (mod == null)
            {
                throw new InvalidOperationException($"Installed mod {installedModId} not found");
            }

            var instance = await _instanceRepository.GetByIdAsync(mod.GameInstanceId);
            if (instance == null)
            {
                throw new InvalidOperationException($"Game instance {mod.GameInstanceId} not found");
            }

            // Get latest version
            var versions = await _modrinthApi.GetProjectVersionsAsync(mod.ModId, instance.MinecraftVersion);
            var latestVersion = versions
                .Where(v => v.GameVersions.Contains(instance.MinecraftVersion))
                .OrderByDescending(v => v.UploadDate)
                .FirstOrDefault();

            if (latestVersion == null)
            {
                throw new InvalidOperationException("No compatible version found");
            }

            // Uninstall current version
            await UninstallModAsync(installedModId);

            // Install new version
            var updatedMod = await InstallModAsync(
                mod.GameInstanceId,
                mod.ModId,
                latestVersion.Id,
                mod.Platform,
                progress);

            _logger.LogInformation("Successfully updated mod {ModName} from {OldVersion} to {NewVersion}",
                mod.Name, mod.Version, updatedMod.Version);

            return updatedMod;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update mod {ModId}", installedModId);
            throw;
        }
    }

    public async Task<ModCompatibilityResult> CheckCompatibilityAsync(
        string gameInstanceId,
        string modId,
        ModPlatform platform)
    {
        try
        {
            _logger.LogInformation("Checking compatibility for mod {ModId} in instance {InstanceId}",
                modId, gameInstanceId);

            var result = new ModCompatibilityResult { IsCompatible = true };

            var instance = await _instanceRepository.GetByIdAsync(gameInstanceId);
            if (instance == null)
            {
                result.IsCompatible = false;
                result.Issues.Add("Game instance not found");
                return result;
            }

            if (platform != ModPlatform.Modrinth)
            {
                result.Issues.Add($"Platform {platform} compatibility check not yet implemented");
                return result;
            }

            // Get mod details
            var mod = await _modrinthApi.GetProjectAsync(modId);
            if (mod == null)
            {
                result.IsCompatible = false;
                result.Issues.Add("Mod not found");
                return result;
            }

            var versions = await _modrinthApi.GetProjectVersionsAsync(modId, instance.MinecraftVersion);

            // Check if any version is compatible with instance's Minecraft version
            var compatibleVersion = versions.FirstOrDefault(v =>
                v.GameVersions.Contains(instance.MinecraftVersion));

            if (compatibleVersion == null)
            {
                result.IsCompatible = false;
                result.Issues.Add($"No version compatible with Minecraft {instance.MinecraftVersion}");
            }

            // Check mod loader compatibility
            if (compatibleVersion != null && instance.ModLoader.HasValue)
            {
                var loaderName = instance.ModLoader.Value.ToString().ToLowerInvariant();
                if (!compatibleVersion.RequiredLoaders.Any(l => l.ToString().Equals(loaderName, StringComparison.OrdinalIgnoreCase)))
                {
                    result.IsCompatible = false;
                    result.Issues.Add($"Mod requires different mod loader. Supported: {string.Join(", ", compatibleVersion.RequiredLoaders)}");
                }
            }

            // Note: Dependency checking would require additional API calls to get ModrinthVersion details
            // This is a simplified version

            _logger.LogInformation("Compatibility check result: {IsCompatible}, Issues: {IssueCount}",
                result.IsCompatible, result.Issues.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check compatibility for mod {ModId}", modId);
            throw;
        }
    }

    public async Task<List<InstalledMod>> GetInstalledModsAsync(string gameInstanceId)
    {
        try
        {
            return await _modRepository.GetByGameInstanceAsync(gameInstanceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get installed mods for instance {InstanceId}", gameInstanceId);
            throw;
        }
    }

    private string GetModsDirectory(GameInstance instance)
    {
        // Mods directory structure: {LocalAppData}/Yuuki/instances/{instance_id}/mods
        var basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Yuuki",
            "instances",
            instance.Id);

        return Path.Combine(basePath, "mods");
    }

    private static string ComputeSha1(byte[] data)
    {
        using var sha1 = System.Security.Cryptography.SHA1.Create();
        var hash = sha1.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
