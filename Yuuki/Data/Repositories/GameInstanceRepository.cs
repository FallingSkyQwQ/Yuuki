using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Yuuki.Models;

namespace Yuuki.Data.Repositories;

/// <summary>
/// Repository for GameInstance entities
/// </summary>
public interface IGameInstanceRepository : IRepository<GameInstance>
{
    /// <summary>
    /// Gets game instance with all installed mods
    /// </summary>
    Task<GameInstance?> GetWithModsAsync(string id);

    /// <summary>
    /// Gets all instances ordered by last played
    /// </summary>
    Task<List<GameInstance>> GetOrderedByLastPlayedAsync();

    /// <summary>
    /// Gets instances for a specific Minecraft version
    /// </summary>
    Task<List<GameInstance>> GetByMinecraftVersionAsync(string version);
}

/// <summary>
/// Implementation of GameInstance repository
/// </summary>
public class GameInstanceRepository : Repository<GameInstance>, IGameInstanceRepository
{
    public GameInstanceRepository(YuukiDbContext context) : base(context)
    {
    }

    public async Task<GameInstance?> GetWithModsAsync(string id)
    {
        return await _dbSet
            .Include(gi => gi.InstalledMods)
            .FirstOrDefaultAsync(gi => gi.Id == id);
    }

    public async Task<List<GameInstance>> GetOrderedByLastPlayedAsync()
    {
        return await _dbSet
            .OrderByDescending(gi => gi.LastPlayed ?? gi.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<GameInstance>> GetByMinecraftVersionAsync(string version)
    {
        return await _dbSet
            .Where(gi => gi.MinecraftVersion == version)
            .ToListAsync();
    }
}
