using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Yuuki.Data.Repositories;
using Yuuki.Exceptions;
using Yuuki.Models;
using Yuuki.Services.Api.Models;

namespace Yuuki.Services.Launch;

/// <summary>
/// Interface for game launcher service
/// </summary>
public interface IGameLauncher
{
    /// <summary>
    /// Launches a game instance
    /// </summary>
    Task<GameProcess> LaunchAsync(
        string gameInstanceId,
        LaunchConfig? config = null,
        IProgress<LaunchProgress>? progress = null);

    /// <summary>
    /// Gets currently running game processes
    /// </summary>
    List<GameProcess> GetRunningGames();

    /// <summary>
    /// Terminates a running game
    /// </summary>
    Task<bool> TerminateGameAsync(int processId);
}

/// <summary>
/// Implementation of game launcher
/// </summary>
public class GameLauncher : IGameLauncher
{
    private readonly IGameInstanceRepository _instanceRepository;
    private readonly IUserAccountRepository _accountRepository;
    private readonly IInstalledModRepository _modRepository;
    private readonly ILogger<GameLauncher> _logger;
    private readonly List<GameProcess> _runningGames = new();
    private readonly string _minecraftBasePath;

    public GameLauncher(
        IGameInstanceRepository instanceRepository,
        IUserAccountRepository accountRepository,
        IInstalledModRepository modRepository,
        ILogger<GameLauncher> logger)
    {
        _instanceRepository = instanceRepository;
        _accountRepository = accountRepository;
        _modRepository = modRepository;
        _logger = logger;

        _minecraftBasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Yuuki",
            "minecraft");
    }

    public async Task<GameProcess> LaunchAsync(
        string gameInstanceId,
        LaunchConfig? config = null,
        IProgress<LaunchProgress>? progress = null)
    {
        try
        {
            _logger.LogInformation("Launching game instance {InstanceId}", gameInstanceId);

            var launchProgress = new LaunchProgress
            {
                Status = "Loading game instance...",
                Step = 1,
                TotalSteps = 7
            };
            progress?.Report(launchProgress);

            // Load game instance
            var instance = await _instanceRepository.GetByIdAsync(gameInstanceId);
            if (instance == null)
            {
                throw new LaunchException("INSTANCE_NOT_FOUND", $"Game instance {gameInstanceId} not found");
            }

            // Use default config if not provided
            config ??= new LaunchConfig();

            launchProgress.Status = "Loading user account...";
            launchProgress.Step = 2;
            progress?.Report(launchProgress);

            // Get active user account
            var account = await _accountRepository.GetActiveAccountAsync();
            if (account == null)
            {
                throw new LaunchException("NO_ACCOUNT", "No active user account. Please log in first.");
            }

            launchProgress.Status = "Preparing game files...";
            launchProgress.Step = 3;
            progress?.Report(launchProgress);

            // Prepare directories
            var gameDir = PrepareGameDirectory(instance);
            var nativesDir = PrepareNativesDirectory(instance);

            launchProgress.Status = "Building class path...";
            launchProgress.Step = 4;
            progress?.Report(launchProgress);

            // Build class path
            var classPath = await BuildClassPathAsync(instance);

            launchProgress.Status = "Generating launch arguments...";
            launchProgress.Step = 5;
            progress?.Report(launchProgress);

            // Generate JVM arguments
            var jvmArgs = GenerateJvmArguments(config, classPath, nativesDir);

            // Generate game arguments
            var gameArgs = await GenerateGameArgumentsAsync(instance, account, config, gameDir);

            launchProgress.Status = "Starting game process...";
            launchProgress.Step = 6;
            progress?.Report(launchProgress);

            // Combine all arguments
            var allArgs = new List<string>();
            allArgs.AddRange(jvmArgs);
            allArgs.AddRange(gameArgs);

            // Start process
            var process = StartGameProcess(config.JavaPath, allArgs, gameDir, config.ShowConsole);

            var gameProcess = new GameProcess
            {
                ProcessId = process.Id,
                GameInstanceId = gameInstanceId,
                StartTime = DateTime.UtcNow
            };

            _runningGames.Add(gameProcess);

            // Monitor process
            MonitorGameProcess(process, gameProcess);

            // Update instance last played time
            instance.LastPlayed = DateTime.UtcNow;
            await _instanceRepository.UpdateAsync(instance);

            launchProgress.Status = "Game started successfully!";
            launchProgress.Step = 7;
            launchProgress.IsComplete = true;
            progress?.Report(launchProgress);

            _logger.LogInformation("Successfully launched game instance {InstanceId} (PID: {ProcessId})",
                gameInstanceId, process.Id);

            return gameProcess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch game instance {InstanceId}", gameInstanceId);

            var failProgress = new LaunchProgress
            {
                IsFailed = true,
                ErrorMessage = ex.Message
            };
            progress?.Report(failProgress);

            throw;
        }
    }

    public List<GameProcess> GetRunningGames()
    {
        return _runningGames.Where(g => g.IsRunning).ToList();
    }

    public async Task<bool> TerminateGameAsync(int processId)
    {
        try
        {
            _logger.LogInformation("Terminating game process {ProcessId}", processId);

            var gameProcess = _runningGames.FirstOrDefault(g => g.ProcessId == processId);
            if (gameProcess == null)
            {
                return false;
            }

            var process = Process.GetProcessById(processId);
            process.Kill();
            await process.WaitForExitAsync();

            gameProcess.IsRunning = false;
            gameProcess.ExitCode = process.ExitCode;

            _logger.LogInformation("Terminated game process {ProcessId}", processId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to terminate game process {ProcessId}", processId);
            return false;
        }
    }

    private string PrepareGameDirectory(GameInstance instance)
    {
        var gameDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Yuuki",
            "instances",
            instance.Id);

        Directory.CreateDirectory(gameDir);
        Directory.CreateDirectory(Path.Combine(gameDir, "saves"));
        Directory.CreateDirectory(Path.Combine(gameDir, "resourcepacks"));
        Directory.CreateDirectory(Path.Combine(gameDir, "shaderpacks"));
        Directory.CreateDirectory(Path.Combine(gameDir, "screenshots"));

        return gameDir;
    }

    private string PrepareNativesDirectory(GameInstance instance)
    {
        var nativesDir = Path.Combine(
            _minecraftBasePath,
            "versions",
            instance.MinecraftVersion,
            "natives");

        Directory.CreateDirectory(nativesDir);
        return nativesDir;
    }

    private async Task<string> BuildClassPathAsync(GameInstance instance)
    {
        var classPathParts = new List<string>();

        // Read version JSON
        var versionJsonPath = Path.Combine(
            _minecraftBasePath,
            "versions",
            instance.MinecraftVersion,
            $"{instance.MinecraftVersion}.json");

        if (!File.Exists(versionJsonPath))
        {
            throw new LaunchException("VERSION_NOT_FOUND", $"Version JSON not found: {versionJsonPath}");
        }

        var versionJson = await File.ReadAllTextAsync(versionJsonPath);
        var versionDetail = JsonSerializer.Deserialize<VersionDetail>(versionJson);
        if (versionDetail == null)
        {
            throw new LaunchException("INVALID_VERSION_JSON", "Failed to parse version JSON");
        }

        // Add libraries to classpath
        foreach (var library in versionDetail.Libraries)
        {
            if (library.Downloads?.Artifact != null)
            {
                var libraryPath = Path.Combine(_minecraftBasePath, "libraries", library.Downloads.Artifact.Path ?? "");
                if (File.Exists(libraryPath))
                {
                    classPathParts.Add(libraryPath);
                }
            }
        }

        // Add client JAR
        var clientJarPath = Path.Combine(
            _minecraftBasePath,
            "versions",
            instance.MinecraftVersion,
            $"{instance.MinecraftVersion}.jar");

        if (!File.Exists(clientJarPath))
        {
            throw new LaunchException("CLIENT_JAR_NOT_FOUND", $"Client JAR not found: {clientJarPath}");
        }

        classPathParts.Add(clientJarPath);

        // Add mods (for Fabric/Forge)
        if (instance.ModLoader.HasValue)
        {
            var modsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Yuuki",
                "instances",
                instance.Id,
                "mods");

            if (Directory.Exists(modsDir))
            {
                var enabledMods = await _modRepository.GetEnabledModsAsync(instance.Id);
                foreach (var mod in enabledMods)
                {
                    var modPath = Path.Combine(modsDir, mod.FileName);
                    if (File.Exists(modPath))
                    {
                        classPathParts.Add(modPath);
                    }
                }
            }
        }

        return string.Join(Path.PathSeparator, classPathParts);
    }

    private List<string> GenerateJvmArguments(LaunchConfig config, string classPath, string nativesDir)
    {
        var args = new List<string>();

        // Memory settings
        args.Add($"-Xmx{config.MaxMemoryMB}M");
        args.Add($"-Xms{config.MinMemoryMB}M");

        // Natives directory
        args.Add($"-Djava.library.path={nativesDir}");

        // Performance optimizations
        args.Add("-XX:+UseG1GC");
        args.Add("-XX:+UnlockExperimentalVMOptions");
        args.Add("-XX:G1NewSizePercent=20");
        args.Add("-XX:G1ReservePercent=20");
        args.Add("-XX:MaxGCPauseMillis=50");
        args.Add("-XX:G1HeapRegionSize=32M");

        // Custom JVM arguments
        args.AddRange(config.CustomJvmArgs);

        // Class path
        args.Add("-cp");
        args.Add(classPath);

        // Main class
        args.Add("net.minecraft.client.main.Main");

        return args;
    }

    private async Task<List<string>> GenerateGameArgumentsAsync(
        GameInstance instance,
        UserAccount account,
        LaunchConfig config,
        string gameDir)
    {
        var args = new List<string>();

        // Username
        args.Add("--username");
        args.Add(account.Username);

        // Version
        args.Add("--version");
        args.Add(instance.MinecraftVersion);

        // Game directory
        args.Add("--gameDir");
        args.Add(gameDir);

        // Assets directory
        args.Add("--assetsDir");
        args.Add(Path.Combine(_minecraftBasePath, "assets"));

        // Asset index
        args.Add("--assetIndex");
        args.Add(instance.MinecraftVersion); // Simplified - should match asset index version

        // UUID
        args.Add("--uuid");
        args.Add(account.Uuid);

        // Access token
        args.Add("--accessToken");
        args.Add(account.AccessToken ?? "");

        // User type
        args.Add("--userType");
        args.Add(account.AccountType == AccountType.Microsoft ? "msa" : "legacy");

        // Version type
        args.Add("--versionType");
        args.Add("release");

        // Window size
        if (!config.Fullscreen)
        {
            args.Add("--width");
            args.Add(config.WindowWidth.ToString());
            args.Add("--height");
            args.Add(config.WindowHeight.ToString());
        }
        else
        {
            args.Add("--fullscreen");
        }

        // Custom game arguments
        args.AddRange(config.CustomGameArgs);

        await Task.CompletedTask; // Placeholder for async operations
        return args;
    }

    private Process StartGameProcess(string javaPath, List<string> arguments, string workingDirectory, bool showConsole)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = javaPath,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = !showConsole
        };

        foreach (var arg in arguments)
        {
            startInfo.ArgumentList.Add(arg);
        }

        var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new LaunchException("PROCESS_START_FAILED", "Failed to start game process");
        }

        return process;
    }

    private void MonitorGameProcess(Process process, GameProcess gameProcess)
    {
        // Monitor stdout
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                gameProcess.LogLines.Add(e.Data);
                _logger.LogDebug("[Game {ProcessId}] {Output}", process.Id, e.Data);

                // Check for crash indicators
                if (e.Data.Contains("Exception") || e.Data.Contains("Error") || e.Data.Contains("Crash"))
                {
                    gameProcess.Crashed = true;
                    gameProcess.CrashReason = e.Data;
                }
            }
        };

        // Monitor stderr
        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                gameProcess.LogLines.Add($"[ERROR] {e.Data}");
                _logger.LogWarning("[Game {ProcessId}] {Error}", process.Id, e.Data);
            }
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // Monitor exit
        Task.Run(async () =>
        {
            await process.WaitForExitAsync();
            gameProcess.IsRunning = false;
            gameProcess.ExitCode = process.ExitCode;

            _logger.LogInformation("Game process {ProcessId} exited with code {ExitCode}",
                process.Id, process.ExitCode);

            if (process.ExitCode != 0 && !gameProcess.Crashed)
            {
                gameProcess.Crashed = true;
                gameProcess.CrashReason = $"Abnormal exit code: {process.ExitCode}";
            }
        });
    }
}
