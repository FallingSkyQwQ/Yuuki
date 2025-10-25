using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Yuuki.Models;

namespace Yuuki.Data.Repositories;

/// <summary>
/// Repository for InstalledMod entities
/// </summary>
public interface IInstalledModRepository : IRepository<InstalledMod>
{
    /// <summary>
    /// Gets all mods for a specific game instance
    /// </summary>
    Task<List<InstalledMod>> GetByGameInstanceAsync(string gameInstanceId);

    /// <summary>
    /// Gets all enabled mods for a game instance
    /// </summary>
    Task<List<InstalledMod>> GetEnabledModsAsync(string gameInstanceId);

    /// <summary>
    /// Gets mods that have updates available
    /// </summary>
    Task<List<InstalledMod>> GetModsWithUpdatesAsync(string gameInstanceId);

    /// <summary>
    /// Checks if a mod is already installed in an instance
    /// </summary>
    Task<bool> IsModInstalledAsync(string gameInstanceId, string modId);
}

/// <summary>
/// Implementation of InstalledMod repository
/// </summary>
public class InstalledModRepository : Repository<InstalledMod>, IInstalledModRepository
{
    public InstalledModRepository(YuukiDbContext context) : base(context)
    {
    }

    public async Task<List<InstalledMod>> GetByGameInstanceAsync(string gameInstanceId)
    {
        return await _dbSet
            .Where(m => m.GameInstanceId == gameInstanceId)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<List<InstalledMod>> GetEnabledModsAsync(string gameInstanceId)
    {
        return await _dbSet
            .Where(m => m.GameInstanceId == gameInstanceId && m.IsEnabled)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<List<InstalledMod>> GetModsWithUpdatesAsync(string gameInstanceId)
    {
        return await _dbSet
            .Where(m => m.GameInstanceId == gameInstanceId && m.HasUpdate)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<bool> IsModInstalledAsync(string gameInstanceId, string modId)
    {
        return await _dbSet
            .AnyAsync(m => m.GameInstanceId == gameInstanceId && m.ModId == modId);
    }
}
