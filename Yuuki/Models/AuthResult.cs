namespace Yuuki.Models;

/// <summary>
/// Result of authentication operation
/// </summary>
public class AuthResult
{
    /// <summary>
    /// Whether authentication was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// User account information if successful
    /// </summary>
    public UserAccount? Account { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Error code if failed
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Creates a successful auth result
    /// </summary>
    public static AuthResult Successful(UserAccount account)
    {
        return new AuthResult
        {
            Success = true,
            Account = account
        };
    }

    /// <summary>
    /// Creates a failed auth result
    /// </summary>
    public static AuthResult Failed(string errorMessage, string? errorCode = null)
    {
        return new AuthResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        };
    }
}
