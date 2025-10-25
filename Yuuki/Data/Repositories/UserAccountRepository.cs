using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Yuuki.Models;

namespace Yuuki.Data.Repositories;

/// <summary>
/// Repository for UserAccount entities
/// </summary>
public interface IUserAccountRepository : IRepository<UserAccount>
{
    /// <summary>
    /// Gets the currently active account
    /// </summary>
    Task<UserAccount?> GetActiveAccountAsync();

    /// <summary>
    /// Sets an account as active (and deactivates others)
    /// </summary>
    Task SetActiveAccountAsync(string accountId);

    /// <summary>
    /// Gets account by UUID
    /// </summary>
    Task<UserAccount?> GetByUuidAsync(string uuid);

    /// <summary>
    /// Gets account by email
    /// </summary>
    Task<UserAccount?> GetByEmailAsync(string email);

    /// <summary>
    /// Gets all accounts ordered by last used
    /// </summary>
    Task<List<UserAccount>> GetOrderedByLastUsedAsync();
}

/// <summary>
/// Implementation of UserAccount repository
/// </summary>
public class UserAccountRepository : Repository<UserAccount>, IUserAccountRepository
{
    public UserAccountRepository(YuukiDbContext context) : base(context)
    {
    }

    public async Task<UserAccount?> GetActiveAccountAsync()
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.IsActive);
    }

    public async Task SetActiveAccountAsync(string accountId)
    {
        // Deactivate all accounts
        var allAccounts = await _dbSet.ToListAsync();
        foreach (var account in allAccounts)
        {
            account.IsActive = false;
        }

        // Activate the specified account
        var targetAccount = await GetByIdAsync(accountId);
        if (targetAccount != null)
        {
            targetAccount.IsActive = true;
            targetAccount.LastUsed = System.DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<UserAccount?> GetByUuidAsync(string uuid)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.Uuid == uuid);
    }

    public async Task<UserAccount?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.Email == email);
    }

    public async Task<List<UserAccount>> GetOrderedByLastUsedAsync()
    {
        return await _dbSet
            .OrderByDescending(a => a.LastUsed ?? a.CreatedAt)
            .ToListAsync();
    }
}
