using System;

namespace Yuuki.Models;

/// <summary>
/// Represents a user account
/// </summary>
public class UserAccount
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Minecraft username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Minecraft UUID
    /// </summary>
    public string Uuid { get; set; } = string.Empty;

    /// <summary>
    /// Microsoft account email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Account type (Microsoft, Offline)
    /// </summary>
    public AccountType AccountType { get; set; } = AccountType.Microsoft;

    /// <summary>
    /// Access token (encrypted)
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Refresh token (encrypted)
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Token expiration time
    /// </summary>
    public DateTime? TokenExpiresAt { get; set; }

    /// <summary>
    /// Profile picture URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// When the account was added
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last time this account was used
    /// </summary>
    public DateTime? LastUsed { get; set; }

    /// <summary>
    /// Whether this is the currently active account
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// Account type enumeration
/// </summary>
public enum AccountType
{
    /// <summary>
    /// Microsoft account (online)
    /// </summary>
    Microsoft,

    /// <summary>
    /// Offline account (for testing/development)
    /// </summary>
    Offline
}
