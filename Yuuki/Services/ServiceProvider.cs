using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Yuuki.Data;
using Yuuki.Data.Repositories;
using Yuuki.Services.Api;
using Yuuki.Services.Authentication;

namespace Yuuki.Services;

/// <summary>
/// Service provider singleton for dependency injection
/// </summary>
public static class ServiceProvider
{
    private static IServiceProvider? _serviceProvider;

    /// <summary>
    /// Gets the configured service provider instance
    /// </summary>
    public static IServiceProvider Current => _serviceProvider
        ?? throw new InvalidOperationException("ServiceProvider has not been initialized. Call Initialize() first.");

    /// <summary>
    /// Initializes the service provider with all required services
    /// </summary>
    public static void Initialize()
    {
        var services = new ServiceCollection();

        // Configure logging
        ConfigureLogging(services);

        // Configure database
        ConfigureDataServices(services);

        // Configure API services
        ConfigureApiServices(services);

        // TODO: Register services here as we build them
        // ConfigureBusinessServices(services);
        // ConfigureViewModels(services);

        _serviceProvider = services.BuildServiceProvider();

        // Initialize database
        InitializeDatabase();
    }

    private static void ConfigureLogging(IServiceCollection services)
    {
        // Configure Serilog
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Yuuki",
            "Logs",
            "yuuki-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });
    }

    private static void ConfigureDataServices(IServiceCollection services)
    {
        // Get database path
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Yuuki",
            "yuuki.db");

        // Ensure directory exists
        var dbDirectory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }

        // Register DbContext
        services.AddDbContext<YuukiDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Register repositories
        services.AddScoped<IGameInstanceRepository, GameInstanceRepository>();
        services.AddScoped<IInstalledModRepository, InstalledModRepository>();
        services.AddScoped<IUserAccountRepository, UserAccountRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    }

    private static void ConfigureApiServices(IServiceCollection services)
    {
        // Register HttpClient for API services
        services.AddHttpClient<IMojangApiService, MojangApiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<IModLoaderApiService, ModLoaderApiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<IModrinthApiService, ModrinthApiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Register authentication services
        services.AddHttpClient<IAccountManager, AccountManager>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });
    }

    private static void InitializeDatabase()
    {
        using var scope = Current.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<YuukiDbContext>();

        // Create database if it doesn't exist
        dbContext.Database.EnsureCreated();
    }

    /// <summary>
    /// Gets a required service from the service provider
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>The service instance</returns>
    public static T GetRequiredService<T>() where T : notnull
    {
        return Current.GetRequiredService<T>();
    }

    /// <summary>
    /// Gets a service from the service provider
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>The service instance or null if not found</returns>
    public static T? GetService<T>()
    {
        return Current.GetService<T>();
    }
}
