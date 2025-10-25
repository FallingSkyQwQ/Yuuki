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

namespace Yuuki.Services.Resource;

/// <summary>
/// Resource pack information
/// </summary>
public class ResourcePack
{
    /// <summary>
    /// Pack file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Pack display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Pack description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Pack format version
    /// </summary>
    public int PackFormat { get; set; }

    /// <summary>
    /// Whether the pack is enabled
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Load order (lower = loads first)
    /// </summary>
    public int LoadOrder { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Installation date
    /// </summary>
    public DateTime InstalledAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Shader pack information
/// </summary>
public class ShaderPack
{
    /// <summary>
    /// Pack file or folder name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a folder (true) or zip file (false)
    /// </summary>
    public bool IsFolder { get; set; }

    /// <summary>
    /// Whether the shader is enabled
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// File/folder size in bytes
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Installation date
    /// </summary>
    public DateTime InstalledAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Interface for resource pack manager
/// </summary>
public interface IResourcePackManager
{
    /// <summary>
    /// Gets all resource packs for a game instance
    /// </summary>
    Task<List<ResourcePack>> GetResourcePacksAsync(string gameInstanceId);

    /// <summary>
    /// Installs a resource pack to a game instance
    /// </summary>
    Task<ResourcePack> InstallResourcePackAsync(string gameInstanceId, string sourceFilePath);

    /// <summary>
    /// Uninstalls a resource pack
    /// </summary>
    Task<bool> UninstallResourcePackAsync(string gameInstanceId, string fileName);

    /// <summary>
    /// Enables or disables a resource pack
    /// </summary>
    Task<bool> ToggleResourcePackAsync(string gameInstanceId, string fileName, bool enabled);

    /// <summary>
    /// Updates resource pack load order
    /// </summary>
    Task<bool> UpdateLoadOrderAsync(string gameInstanceId, List<string> orderedFileNames);

    /// <summary>
    /// Gets enabled resource packs in load order
    /// </summary>
    Task<List<ResourcePack>> GetEnabledPacksInOrderAsync(string gameInstanceId);
}

/// <summary>
/// Interface for shader pack manager
/// </summary>
public interface IShaderPackManager
{
    /// <summary>
    /// Gets all shader packs for a game instance
    /// </summary>
    Task<List<ShaderPack>> GetShaderPacksAsync(string gameInstanceId);

    /// <summary>
    /// Installs a shader pack to a game instance
    /// </summary>
    Task<ShaderPack> InstallShaderPackAsync(string gameInstanceId, string sourceFilePath);

    /// <summary>
    /// Uninstalls a shader pack
    /// </summary>
    Task<bool> UninstallShaderPackAsync(string gameInstanceId, string name);

    /// <summary>
    /// Enables a shader pack (only one can be enabled at a time)
    /// </summary>
    Task<bool> EnableShaderPackAsync(string gameInstanceId, string name);

    /// <summary>
    /// Disables all shader packs
    /// </summary>
    Task<bool> DisableAllShadersAsync(string gameInstanceId);
}

/// <summary>
/// Implementation of resource pack manager
/// </summary>
public class ResourcePackManager : IResourcePackManager
{
    private readonly IGameInstanceRepository _instanceRepository;
    private readonly ILogger<ResourcePackManager> _logger;

    public ResourcePackManager(
        IGameInstanceRepository instanceRepository,
        ILogger<ResourcePackManager> logger)
    {
        _instanceRepository = instanceRepository;
        _logger = logger;
    }

    public async Task<List<ResourcePack>> GetResourcePacksAsync(string gameInstanceId)
    {
        try
        {
            var instance = await _instanceRepository.GetByIdAsync(gameInstanceId);
            if (instance == null)
            {
                return new List<ResourcePack>();
            }

            var resourcePacksDir = GetResourcePacksDirectory(instance);
            if (!Directory.Exists(resourcePacksDir))
            {
                return new List<ResourcePack>();
            }

            var packs = new List<ResourcePack>();
            var files = Directory.GetFiles(resourcePacksDir, "*.zip");

            foreach (var file in files)
            {
                var pack = await ReadResourcePackInfoAsync(file);
                if (pack != null)
                {
                    packs.Add(pack);
                }
            }

            // Load enabled packs from options.txt
            var enabledPacks = await LoadEnabledPacksAsync(instance);
            foreach (var pack in packs)
            {
                pack.IsEnabled = enabledPacks.Contains(pack.FileName);
                pack.LoadOrder = enabledPacks.IndexOf(pack.FileName);
            }

            return packs.OrderBy(p => p.LoadOrder).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get resource packs for instance {InstanceId}", gameInstanceId);
            return new List<ResourcePack>();
        }
    }

    public async Task<ResourcePack> InstallResourcePackAsync(string gameInstanceId, string sourceFilePath)
    {
        try
        {
            _logger.LogInformation("Installing resource pack {SourceFile} to instance {InstanceId}",
                sourceFilePath, gameInstanceId);

            var instance = await _instanceRepository.GetByIdAsync(gameInstanceId);
            if (instance == null)
            {
                throw new InvalidOperationException($"Game instance {gameInstanceId} not found");
            }

            var resourcePacksDir = GetResourcePacksDirectory(instance);
            Directory.CreateDirectory(resourcePacksDir);

            var fileName = Path.GetFileName(sourceFilePath);
            var destPath = Path.Combine(resourcePacksDir, fileName);

            // Copy file
            File.Copy(sourceFilePath, destPath, overwrite: true);

            // Read pack info
            var pack = await ReadResourcePackInfoAsync(destPath);
            if (pack == null)
            {
                throw new InvalidOperationException("Failed to read resource pack info");
            }

            _logger.LogInformation("Successfully installed resource pack {PackName}", pack.Name);
            return pack;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to install resource pack");
            throw;
        }
    }

    public async Task<bool> UninstallResourcePackAsync(string gameInstanceId, string fileName)
    {
        try
        {
            _logger.LogInformation("Uninstalling resource pack {FileName} from instance {InstanceId}",
                fileName, gameInstanceId);

            var instance = await _instanceRepository.GetByIdAsync(gameInstanceId);
            if (instance == null)
            {
                return false;
            }

            var resourcePacksDir = GetResourcePacksDirectory(instance);
            var filePath = Path.Combine(resourcePacksDir, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);

                // Remove from enabled packs
                await RemoveFromEnabledPacksAsync(instance, fileName);

                _logger.LogInformation("Successfully uninstalled resource pack {FileName}", fileName);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to uninstall resource pack {FileName}", fileName);
            return false;
        }
    }

    public async Task<bool> ToggleResourcePackAsync(string gameInstanceId, string fileName, bool enabled)
    {
        try
        {
            var instance = await _instanceRepository.GetByIdAsync(gameInstanceId);
            if (instance == null)
            {
                return false;
            }

            var enabledPacks = await LoadEnabledPacksAsync(instance);

            if (enabled)
            {
                if (!enabledPacks.Contains(fileName))
                {
                    enabledPacks.Add(fileName);
                }
            }
            else
            {
                enabledPacks.Remove(fileName);
            }

            await SaveEnabledPacksAsync(instance, enabledPacks);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle resource pack {FileName}", fileName);
            return false;
        }
    }

    public async Task<bool> UpdateLoadOrderAsync(string gameInstanceId, List<string> orderedFileNames)
    {
        try
        {
            var instance = await _instanceRepository.GetByIdAsync(gameInstanceId);
            if (instance == null)
            {
                return false;
            }

            await SaveEnabledPacksAsync(instance, orderedFileNames);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update load order for instance {InstanceId}", gameInstanceId);
            return false;
        }
    }

    public async Task<List<ResourcePack>> GetEnabledPacksInOrderAsync(string gameInstanceId)
    {
        var allPacks = await GetResourcePacksAsync(gameInstanceId);
        return allPacks.Where(p => p.IsEnabled).OrderBy(p => p.LoadOrder).ToList();
    }

    private string GetResourcePacksDirectory(GameInstance instance)
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Yuuki",
            "instances",
            instance.Id,
            "resourcepacks");
    }

    private async Task<ResourcePack?> ReadResourcePackInfoAsync(string filePath)
    {
        try
        {
            using var archive = ZipFile.OpenRead(filePath);
            var packMcmetaEntry = archive.GetEntry("pack.mcmeta");

            if (packMcmetaEntry == null)
            {
                // Create basic pack info if pack.mcmeta doesn't exist
                return new ResourcePack
                {
                    FileName = Path.GetFileName(filePath),
                    Name = Path.GetFileNameWithoutExtension(filePath),
                    Description = "No description",
                    PackFormat = 0,
                    FileSize = new FileInfo(filePath).Length,
                    InstalledAt = File.GetCreationTime(filePath)
                };
            }

            using var stream = packMcmetaEntry.Open();
            using var reader = new StreamReader(stream);
            var jsonContent = await reader.ReadToEndAsync();
            var packMeta = JsonSerializer.Deserialize<JsonDocument>(jsonContent);

            if (packMeta == null)
            {
                return null;
            }

            var pack = packMeta.RootElement.GetProperty("pack");
            var description = pack.TryGetProperty("description", out var desc)
                ? desc.GetString() ?? "No description"
                : "No description";
            var packFormat = pack.TryGetProperty("pack_format", out var format)
                ? format.GetInt32()
                : 0;

            return new ResourcePack
            {
                FileName = Path.GetFileName(filePath),
                Name = Path.GetFileNameWithoutExtension(filePath),
                Description = description,
                PackFormat = packFormat,
                FileSize = new FileInfo(filePath).Length,
                InstalledAt = File.GetCreationTime(filePath)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read pack.mcmeta from {FilePath}", filePath);
            return null;
        }
    }

    private async Task<List<string>> LoadEnabledPacksAsync(GameInstance instance)
    {
        var gameDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Yuuki",
            "instances",
            instance.Id);

        var optionsPath = Path.Combine(gameDir, "options.txt");
        if (!File.Exists(optionsPath))
        {
            return new List<string>();
        }

        var lines = await File.ReadAllLinesAsync(optionsPath);
        var resourcePacksLine = lines.FirstOrDefault(l => l.StartsWith("resourcePacks:"));

        if (resourcePacksLine == null)
        {
            return new List<string>();
        }

        // Parse JSON array from options.txt
        var jsonPart = resourcePacksLine.Substring("resourcePacks:".Length);
        try
        {
            var packs = JsonSerializer.Deserialize<List<string>>(jsonPart);
            return packs ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private async Task SaveEnabledPacksAsync(GameInstance instance, List<string> enabledPacks)
    {
        var gameDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Yuuki",
            "instances",
            instance.Id);

        var optionsPath = Path.Combine(gameDir, "options.txt");

        List<string> lines;
        if (File.Exists(optionsPath))
        {
            lines = (await File.ReadAllLinesAsync(optionsPath)).ToList();
        }
        else
        {
            Directory.CreateDirectory(gameDir);
            lines = new List<string>();
        }

        // Update or add resourcePacks line
        var json = JsonSerializer.Serialize(enabledPacks);
        var newLine = $"resourcePacks:{json}";

        var existingIndex = lines.FindIndex(l => l.StartsWith("resourcePacks:"));
        if (existingIndex >= 0)
        {
            lines[existingIndex] = newLine;
        }
        else
        {
            lines.Add(newLine);
        }

        await File.WriteAllLinesAsync(optionsPath, lines);
    }

    private async Task RemoveFromEnabledPacksAsync(GameInstance instance, string fileName)
    {
        var enabledPacks = await LoadEnabledPacksAsync(instance);
        enabledPacks.Remove(fileName);
        await SaveEnabledPacksAsync(instance, enabledPacks);
    }
}

/// <summary>
/// Implementation of shader pack manager
/// </summary>
public class ShaderPackManager : IShaderPackManager
{
    private readonly IGameInstanceRepository _instanceRepository;
    private readonly ILogger<ShaderPackManager> _logger;

    public ShaderPackManager(
        IGameInstanceRepository instanceRepository,
        ILogger<ShaderPackManager> logger)
    {
        _instanceRepository = instanceRepository;
        _logger = logger;
    }

    public async Task<List<ShaderPack>> GetShaderPacksAsync(string gameInstanceId)
    {
        try
        {
            var instance = await _instanceRepository.GetByIdAsync(gameInstanceId);
            if (instance == null)
            {
                return new List<ShaderPack>();
            }

            var shaderPacksDir = GetShaderPacksDirectory(instance);
            if (!Directory.Exists(shaderPacksDir))
            {
                return new List<ShaderPack>();
            }

            var packs = new List<ShaderPack>();

            // Get zip files
            var zipFiles = Directory.GetFiles(shaderPacksDir, "*.zip");
            foreach (var file in zipFiles)
            {
                packs.Add(new ShaderPack
                {
                    Name = Path.GetFileName(file),
                    IsFolder = false,
                    Size = new FileInfo(file).Length,
                    InstalledAt = File.GetCreationTime(file)
                });
            }

            // Get folders
            var folders = Directory.GetDirectories(shaderPacksDir);
            foreach (var folder in folders)
            {
                var dirInfo = new DirectoryInfo(folder);
                packs.Add(new ShaderPack
                {
                    Name = dirInfo.Name,
                    IsFolder = true,
                    Size = GetDirectorySize(dirInfo),
                    InstalledAt = dirInfo.CreationTime
                });
            }

            // Load enabled shader from shaderpacks config
            var enabledShader = await LoadEnabledShaderAsync(instance);
            foreach (var pack in packs)
            {
                pack.IsEnabled = pack.Name == enabledShader;
            }

            return packs.OrderBy(p => p.Name).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get shader packs for instance {InstanceId}", gameInstanceId);
            return new List<ShaderPack>();
        }
    }

    public async Task<ShaderPack> InstallShaderPackAsync(string gameInstanceId, string sourceFilePath)
    {
        try
        {
            _logger.LogInformation("Installing shader pack {SourceFile} to instance {InstanceId}",
                sourceFilePath, gameInstanceId);

            var instance = await _instanceRepository.GetByIdAsync(gameInstanceId);
            if (instance == null)
            {
                throw new InvalidOperationException($"Game instance {gameInstanceId} not found");
            }

            var shaderPacksDir = GetShaderPacksDirectory(instance);
            Directory.CreateDirectory(shaderPacksDir);

            var fileName = Path.GetFileName(sourceFilePath);
            var destPath = Path.Combine(shaderPacksDir, fileName);

            // Copy file
            File.Copy(sourceFilePath, destPath, overwrite: true);

            var pack = new ShaderPack
            {
                Name = fileName,
                IsFolder = false,
                Size = new FileInfo(destPath).Length,
                InstalledAt = DateTime.UtcNow
            };

            _logger.LogInformation("Successfully installed shader pack {PackName}", pack.Name);
            return pack;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to install shader pack");
            throw;
        }
    }

    public async Task<bool> UninstallShaderPackAsync(string gameInstanceId, string name)
    {
        try
        {
            _logger.LogInformation("Uninstalling shader pack {Name} from instance {InstanceId}",
                name, gameInstanceId);

            var instance = await _instanceRepository.GetByIdAsync(gameInstanceId);
            if (instance == null)
            {
                return false;
            }

            var shaderPacksDir = GetShaderPacksDirectory(instance);
            var filePath = Path.Combine(shaderPacksDir, name);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            else if (Directory.Exists(filePath))
            {
                Directory.Delete(filePath, recursive: true);
            }
            else
            {
                return false;
            }

            // Disable if it was enabled
            var enabledShader = await LoadEnabledShaderAsync(instance);
            if (enabledShader == name)
            {
                await SaveEnabledShaderAsync(instance, null);
            }

            _logger.LogInformation("Successfully uninstalled shader pack {Name}", name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to uninstall shader pack {Name}", name);
            return false;
        }
    }

    public async Task<bool> EnableShaderPackAsync(string gameInstanceId, string name)
    {
        try
        {
            var instance = await _instanceRepository.GetByIdAsync(gameInstanceId);
            if (instance == null)
            {
                return false;
            }

            await SaveEnabledShaderAsync(instance, name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enable shader pack {Name}", name);
            return false;
        }
    }

    public async Task<bool> DisableAllShadersAsync(string gameInstanceId)
    {
        try
        {
            var instance = await _instanceRepository.GetByIdAsync(gameInstanceId);
            if (instance == null)
            {
                return false;
            }

            await SaveEnabledShaderAsync(instance, null);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disable shaders for instance {InstanceId}", gameInstanceId);
            return false;
        }
    }

    private string GetShaderPacksDirectory(GameInstance instance)
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Yuuki",
            "instances",
            instance.Id,
            "shaderpacks");
    }

    private async Task<string?> LoadEnabledShaderAsync(GameInstance instance)
    {
        var gameDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Yuuki",
            "instances",
            instance.Id);

        var optionsPath = Path.Combine(gameDir, "optionsshaders.txt");
        if (!File.Exists(optionsPath))
        {
            return null;
        }

        var lines = await File.ReadAllLinesAsync(optionsPath);
        var shaderPackLine = lines.FirstOrDefault(l => l.StartsWith("shaderPack="));

        if (shaderPackLine == null)
        {
            return null;
        }

        var shaderName = shaderPackLine.Substring("shaderPack=".Length);
        return string.IsNullOrEmpty(shaderName) ? null : shaderName;
    }

    private async Task SaveEnabledShaderAsync(GameInstance instance, string? shaderName)
    {
        var gameDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Yuuki",
            "instances",
            instance.Id);

        var optionsPath = Path.Combine(gameDir, "optionsshaders.txt");

        List<string> lines;
        if (File.Exists(optionsPath))
        {
            lines = (await File.ReadAllLinesAsync(optionsPath)).ToList();
        }
        else
        {
            Directory.CreateDirectory(gameDir);
            lines = new List<string>();
        }

        // Update or add shaderPack line
        var newLine = $"shaderPack={shaderName ?? ""}";

        var existingIndex = lines.FindIndex(l => l.StartsWith("shaderPack="));
        if (existingIndex >= 0)
        {
            lines[existingIndex] = newLine;
        }
        else
        {
            lines.Add(newLine);
        }

        await File.WriteAllLinesAsync(optionsPath, lines);
    }

    private long GetDirectorySize(DirectoryInfo dirInfo)
    {
        long size = 0;

        try
        {
            var files = dirInfo.GetFiles("*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                size += file.Length;
            }
        }
        catch
        {
            // Ignore errors
        }

        return size;
    }
}
