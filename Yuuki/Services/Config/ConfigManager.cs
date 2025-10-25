using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Yuuki.Data.Repositories;
using Yuuki.Models;

namespace Yuuki.Services.Config;

/// <summary>
/// Application settings
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Default Java path
    /// </summary>
    public string JavaPath { get; set; } = "java";

    /// <summary>
    /// Default max memory in MB
    /// </summary>
    public int DefaultMaxMemoryMB { get; set; } = 2048;

    /// <summary>
    /// Default min memory in MB
    /// </summary>
    public int DefaultMinMemoryMB { get; set; } = 512;

    /// <summary>
    /// Theme (Light/Dark)
    /// </summary>
    public string Theme { get; set; } = "Dark";

    /// <summary>
    /// Language code
    /// </summary>
    public string Language { get; set; } = "zh-CN";

    /// <summary>
    /// Download threads
    /// </summary>
    public int DownloadThreads { get; set; } = 4;

    /// <summary>
    /// Auto check for updates
    /// </summary>
    public bool AutoCheckUpdates { get; set; } = true;

    /// <summary>
    /// Keep game running after launcher closes
    /// </summary>
    public bool KeepLauncherOpen { get; set; } = false;
}

/// <summary>
/// Backup information
/// </summary>
public class BackupInfo
{
    /// <summary>
    /// Backup ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Game instance ID
    /// </summary>
    public string GameInstanceId { get; set; } = string.Empty;

    /// <summary>
    /// Game instance name
    /// </summary>
    public string InstanceName { get; set; } = string.Empty;

    /// <summary>
    /// Backup file path
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Backup creation time
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Backup size in bytes
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Backup description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Backup type (Full/Saves/Config)
    /// </summary>
    public BackupType Type { get; set; } = BackupType.Full;
}

/// <summary>
/// Backup types
/// </summary>
public enum BackupType
{
    /// <summary>
    /// Full instance backup
    /// </summary>
    Full,

    /// <summary>
    /// Saves only
    /// </summary>
    SavesOnly,

    /// <summary>
    /// Configuration only
    /// </summary>
    ConfigOnly
}

/// <summary>
/// Interface for configuration manager
/// </summary>
public interface IConfigManager
{
    /// <summary>
    /// Gets application settings
    /// </summary>
    Task<AppSettings> GetSettingsAsync();

    /// <summary>
    /// Saves application settings
    /// </summary>
    Task SaveSettingsAsync(AppSettings settings);

    /// <summary>
    /// Resets settings to default
    /// </summary>
    Task ResetSettingsAsync();
}

/// <summary>
/// Interface for backup manager
/// </summary>
public interface IBackupManager
{
    /// <summary>
    /// Creates a backup of a game instance
    /// </summary>
    Task<BackupInfo> CreateBackupAsync(
        string gameInstanceId,
        BackupType type = BackupType.Full,
        string? description = null,
        IProgress<int>? progress = null);

    /// <summary>
    /// Restores a backup
    /// </summary>
    Task<bool> RestoreBackupAsync(string backupId, IProgress<int>? progress = null);

    /// <summary>
    /// Deletes a backup
    /// </summary>
    Task<bool> DeleteBackupAsync(string backupId);

    /// <summary>
    /// Gets all backups
    /// </summary>
    Task<List<BackupInfo>> GetBackupsAsync();

    /// <summary>
    /// Gets backups for a specific instance
    /// </summary>
    Task<List<BackupInfo>> GetBackupsForInstanceAsync(string gameInstanceId);

    /// <summary>
    /// Exports a game instance configuration
    /// </summary>
    Task<string> ExportInstanceAsync(string gameInstanceId, string exportPath);

    /// <summary>
    /// Imports a game instance configuration
    /// </summary>
    Task<GameInstance> ImportInstanceAsync(string importPath);
}

/// <summary>
/// Implementation of configuration manager
/// </summary>
public class ConfigManager : IConfigManager
{
    private readonly ILogger<ConfigManager> _logger;
    private readonly string _settingsPath;

    public ConfigManager(ILogger<ConfigManager> logger)
    {
        _logger = logger;

        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Yuuki");

        Directory.CreateDirectory(appDataPath);
        _settingsPath = Path.Combine(appDataPath, "settings.json");
    }

    public async Task<AppSettings> GetSettingsAsync()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return new AppSettings();
            }

            var json = await File.ReadAllTextAsync(_settingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            return settings ?? new AppSettings();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings");
            return new AppSettings();
        }
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(settings, options);
            await File.WriteAllTextAsync(_settingsPath, json);

            _logger.LogInformation("Settings saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings");
            throw;
        }
    }

    public async Task ResetSettingsAsync()
    {
        try
        {
            var defaultSettings = new AppSettings();
            await SaveSettingsAsync(defaultSettings);

            _logger.LogInformation("Settings reset to default");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset settings");
            throw;
        }
    }
}

/// <summary>
/// Implementation of backup manager
/// </summary>
public class BackupManager : IBackupManager
{
    private readonly IGameInstanceRepository _instanceRepository;
    private readonly ILogger<BackupManager> _logger;
    private readonly string _backupsPath;
    private readonly string _backupsIndexPath;

    public BackupManager(
        IGameInstanceRepository instanceRepository,
        ILogger<BackupManager> logger)
    {
        _instanceRepository = instanceRepository;
        _logger = logger;

        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Yuuki");

        _backupsPath = Path.Combine(appDataPath, "backups");
        Directory.CreateDirectory(_backupsPath);

        _backupsIndexPath = Path.Combine(appDataPath, "backups.json");
    }

    public async Task<BackupInfo> CreateBackupAsync(
        string gameInstanceId,
        BackupType type = BackupType.Full,
        string? description = null,
        IProgress<int>? progress = null)
    {
        try
        {
            _logger.LogInformation("Creating {Type} backup for instance {InstanceId}", type, gameInstanceId);

            var instance = await _instanceRepository.GetByIdAsync(gameInstanceId);
            if (instance == null)
            {
                throw new InvalidOperationException($"Game instance {gameInstanceId} not found");
            }

            progress?.Report(10);

            var instanceDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Yuuki",
                "instances",
                instance.Id);

            if (!Directory.Exists(instanceDir))
            {
                throw new InvalidOperationException($"Instance directory not found: {instanceDir}");
            }

            // Create backup info
            var backup = new BackupInfo
            {
                GameInstanceId = gameInstanceId,
                InstanceName = instance.Name,
                Description = description ?? $"{type} backup of {instance.Name}",
                Type = type,
                CreatedAt = DateTime.UtcNow
            };

            var backupFileName = $"{instance.Name}_{backup.Id}_{type}.zip";
            backup.FilePath = Path.Combine(_backupsPath, backupFileName);

            progress?.Report(30);

            // Create zip archive
            using (var archive = ZipFile.Open(backup.FilePath, ZipArchiveMode.Create))
            {
                switch (type)
                {
                    case BackupType.Full:
                        AddDirectoryToArchive(archive, instanceDir, "", progress, 30, 80);
                        break;

                    case BackupType.SavesOnly:
                        var savesDir = Path.Combine(instanceDir, "saves");
                        if (Directory.Exists(savesDir))
                        {
                            AddDirectoryToArchive(archive, savesDir, "saves", progress, 30, 80);
                        }
                        break;

                    case BackupType.ConfigOnly:
                        var configFiles = new[] { "options.txt", "optionsof.txt", "optionsshaders.txt", "servers.dat" };
                        foreach (var configFile in configFiles)
                        {
                            var configPath = Path.Combine(instanceDir, configFile);
                            if (File.Exists(configPath))
                            {
                                archive.CreateEntryFromFile(configPath, configFile);
                            }
                        }
                        break;
                }
            }

            progress?.Report(90);

            backup.Size = new FileInfo(backup.FilePath).Length;

            // Save to index
            await AddBackupToIndexAsync(backup);

            progress?.Report(100);

            _logger.LogInformation("Backup created successfully: {BackupFile}", backup.FilePath);
            return backup;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create backup for instance {InstanceId}", gameInstanceId);
            throw;
        }
    }

    public async Task<bool> RestoreBackupAsync(string backupId, IProgress<int>? progress = null)
    {
        try
        {
            _logger.LogInformation("Restoring backup {BackupId}", backupId);

            var backups = await LoadBackupsIndexAsync();
            var backup = backups.FirstOrDefault(b => b.Id == backupId);

            if (backup == null || !File.Exists(backup.FilePath))
            {
                _logger.LogWarning("Backup {BackupId} not found", backupId);
                return false;
            }

            progress?.Report(10);

            var instanceDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Yuuki",
                "instances",
                backup.GameInstanceId);

            progress?.Report(30);

            // Extract backup
            using (var archive = ZipFile.OpenRead(backup.FilePath))
            {
                var totalEntries = archive.Entries.Count;
                var extractedEntries = 0;

                foreach (var entry in archive.Entries)
                {
                    var destinationPath = Path.Combine(instanceDir, entry.FullName);
                    var destinationDir = Path.GetDirectoryName(destinationPath);

                    if (!string.IsNullOrEmpty(destinationDir))
                    {
                        Directory.CreateDirectory(destinationDir);
                    }

                    if (!string.IsNullOrEmpty(entry.Name))
                    {
                        entry.ExtractToFile(destinationPath, overwrite: true);
                    }

                    extractedEntries++;
                    var percentage = 30 + (int)((double)extractedEntries / totalEntries * 60);
                    progress?.Report(percentage);
                }
            }

            progress?.Report(100);

            _logger.LogInformation("Backup restored successfully: {BackupId}", backupId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore backup {BackupId}", backupId);
            return false;
        }
    }

    public async Task<bool> DeleteBackupAsync(string backupId)
    {
        try
        {
            _logger.LogInformation("Deleting backup {BackupId}", backupId);

            var backups = await LoadBackupsIndexAsync();
            var backup = backups.FirstOrDefault(b => b.Id == backupId);

            if (backup == null)
            {
                return false;
            }

            // Delete backup file
            if (File.Exists(backup.FilePath))
            {
                File.Delete(backup.FilePath);
            }

            // Remove from index
            backups.Remove(backup);
            await SaveBackupsIndexAsync(backups);

            _logger.LogInformation("Backup deleted: {BackupId}", backupId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete backup {BackupId}", backupId);
            return false;
        }
    }

    public async Task<List<BackupInfo>> GetBackupsAsync()
    {
        return await LoadBackupsIndexAsync();
    }

    public async Task<List<BackupInfo>> GetBackupsForInstanceAsync(string gameInstanceId)
    {
        var allBackups = await LoadBackupsIndexAsync();
        return allBackups.Where(b => b.GameInstanceId == gameInstanceId)
            .OrderByDescending(b => b.CreatedAt)
            .ToList();
    }

    public async Task<string> ExportInstanceAsync(string gameInstanceId, string exportPath)
    {
        try
        {
            _logger.LogInformation("Exporting instance {InstanceId} to {ExportPath}", gameInstanceId, exportPath);

            var instance = await _instanceRepository.GetByIdAsync(gameInstanceId);
            if (instance == null)
            {
                throw new InvalidOperationException($"Game instance {gameInstanceId} not found");
            }

            var instanceDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Yuuki",
                "instances",
                instance.Id);

            var exportFileName = $"{instance.Name}_export.zip";
            var fullExportPath = Path.Combine(exportPath, exportFileName);

            // Create export zip
            if (File.Exists(fullExportPath))
            {
                File.Delete(fullExportPath);
            }

            ZipFile.CreateFromDirectory(instanceDir, fullExportPath, CompressionLevel.Optimal, false);

            _logger.LogInformation("Instance exported successfully: {ExportPath}", fullExportPath);
            return fullExportPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export instance {InstanceId}", gameInstanceId);
            throw;
        }
    }

    public async Task<GameInstance> ImportInstanceAsync(string importPath)
    {
        try
        {
            _logger.LogInformation("Importing instance from {ImportPath}", importPath);

            if (!File.Exists(importPath))
            {
                throw new FileNotFoundException($"Import file not found: {importPath}");
            }

            // Create new instance
            var instance = new GameInstance
            {
                Id = Guid.NewGuid().ToString(),
                Name = Path.GetFileNameWithoutExtension(importPath),
                CreatedAt = DateTime.UtcNow
            };

            var instanceDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Yuuki",
                "instances",
                instance.Id);

            // Extract import
            ZipFile.ExtractToDirectory(importPath, instanceDir, overwriteFiles: true);

            // Save to repository
            await _instanceRepository.AddAsync(instance);

            _logger.LogInformation("Instance imported successfully: {InstanceId}", instance.Id);
            return instance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import instance from {ImportPath}", importPath);
            throw;
        }
    }

    private void AddDirectoryToArchive(
        ZipArchive archive,
        string sourceDir,
        string entryPrefix,
        IProgress<int>? progress,
        int progressStart,
        int progressEnd)
    {
        var files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
        var totalFiles = files.Length;
        var processedFiles = 0;

        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(sourceDir, file);
            var entryName = string.IsNullOrEmpty(entryPrefix)
                ? relativePath
                : Path.Combine(entryPrefix, relativePath);

            archive.CreateEntryFromFile(file, entryName.Replace('\\', '/'));

            processedFiles++;
            if (progress != null && totalFiles > 0)
            {
                var percentage = progressStart + (int)((double)processedFiles / totalFiles * (progressEnd - progressStart));
                progress.Report(percentage);
            }
        }
    }

    private async Task<List<BackupInfo>> LoadBackupsIndexAsync()
    {
        try
        {
            if (!File.Exists(_backupsIndexPath))
            {
                return new List<BackupInfo>();
            }

            var json = await File.ReadAllTextAsync(_backupsIndexPath);
            var backups = JsonSerializer.Deserialize<List<BackupInfo>>(json);
            return backups ?? new List<BackupInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load backups index");
            return new List<BackupInfo>();
        }
    }

    private async Task SaveBackupsIndexAsync(List<BackupInfo> backups)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(backups, options);
            await File.WriteAllTextAsync(_backupsIndexPath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save backups index");
            throw;
        }
    }

    private async Task AddBackupToIndexAsync(BackupInfo backup)
    {
        var backups = await LoadBackupsIndexAsync();
        backups.Add(backup);
        await SaveBackupsIndexAsync(backups);
    }
}
