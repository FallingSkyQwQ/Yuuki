using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Yuuki.Exceptions;
using Yuuki.Models;

namespace Yuuki.Services.Authentication;

/// <summary>
/// Interface for account manager
/// </summary>
public interface IAccountManager
{
    /// <summary>
    /// Authenticates with Microsoft account
    /// </summary>
    Task<AuthResult> AuthenticateAsync();

    /// <summary>
    /// Refreshes the access token
    /// </summary>
    Task<bool> RefreshTokenAsync(UserAccount account);

    /// <summary>
    /// Gets the Minecraft profile for an authenticated user
    /// </summary>
    Task<UserAccount?> GetMinecraftProfileAsync(string accessToken);

    /// <summary>
    /// Signs out the current account
    /// </summary>
    Task SignOutAsync(UserAccount account);

    /// <summary>
    /// Whether any account is authenticated
    /// </summary>
    bool IsAuthenticated { get; }
}

/// <summary>
/// Implementation of account manager using MSAL
/// </summary>
public class AccountManager : IAccountManager
{
    private const string ClientId = "11d9b2c5-9ab2-4493-a9de-2517e39981ec"; // my client id
    private const string Authority = "https://login.microsoftonline.com/consumers";
    private static readonly string[] Scopes = new[] { "XboxLive.signin", "offline_access" };

    private const string XboxAuthUrl = "https://user.auth.xboxlive.com/user/authenticate";
    private const string XstsAuthUrl = "https://xsts.auth.xboxlive.com/xsts/authorize";
    private const string MinecraftAuthUrl = "https://api.minecraftservices.com/authentication/login_with_xbox";
    private const string MinecraftProfileUrl = "https://api.minecraftservices.com/minecraft/profile";

    private readonly IPublicClientApplication _msalClient;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AccountManager> _logger;

    public bool IsAuthenticated { get; private set; }

    public AccountManager(HttpClient httpClient, ILogger<AccountManager> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Initialize MSAL client
        _msalClient = PublicClientApplicationBuilder
            .Create(ClientId)
            .WithAuthority(Authority)
            .WithRedirectUri("http://localhost") // For desktop apps
            .Build();
    }

    public async Task<AuthResult> AuthenticateAsync()
    {
        try
        {
            _logger.LogInformation("Starting Microsoft account authentication");

            // Step 1: Get Microsoft access token
            var msalResult = await _msalClient
                .AcquireTokenInteractive(Scopes)
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync();

            _logger.LogInformation("Microsoft authentication successful for {Username}", msalResult.Account.Username);

            // Step 2: Xbox Live authentication
            var xboxToken = await AuthenticateXboxLiveAsync(msalResult.AccessToken);
            if (xboxToken == null)
            {
                return AuthResult.Failed("Failed to authenticate with Xbox Live");
            }

            // Step 3: XSTS authentication
            var xstsToken = await AuthenticateXstsAsync(xboxToken);
            if (xstsToken == null)
            {
                return AuthResult.Failed("Failed to authenticate with XSTS");
            }

            // Step 4: Minecraft authentication
            var minecraftToken = await AuthenticateMinecraftAsync(xstsToken.Token, xstsToken.UserHash);
            if (minecraftToken == null)
            {
                return AuthResult.Failed("Failed to authenticate with Minecraft");
            }

            // Step 5: Get Minecraft profile
            var profile = await GetMinecraftProfileAsync(minecraftToken);
            if (profile == null)
            {
                return AuthResult.Failed("Failed to retrieve Minecraft profile. You may not own Minecraft.");
            }

            // Create user account
            var account = new UserAccount
            {
                Username = profile.Username,
                Uuid = profile.Uuid,
                Email = msalResult.Account.Username,
                AccountType = AccountType.Microsoft,
                AccessToken = minecraftToken,
                RefreshToken = msalResult.Account.HomeAccountId.Identifier,
                TokenExpiresAt = msalResult.ExpiresOn.UtcDateTime,
                IsActive = true
            };

            IsAuthenticated = true;
            _logger.LogInformation("Authentication completed successfully for {Username}", account.Username);

            return AuthResult.Successful(account);
        }
        catch (MsalException ex)
        {
            _logger.LogError(ex, "MSAL authentication failed");
            return AuthResult.Failed($"Authentication failed: {ex.Message}", ex.ErrorCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed");
            return AuthResult.Failed($"Authentication failed: {ex.Message}");
        }
    }

    public async Task<bool> RefreshTokenAsync(UserAccount account)
    {
        try
        {
            _logger.LogInformation("Refreshing token for {Username}", account.Username);

            var accounts = await _msalClient.GetAccountsAsync();
            var msalAccount = accounts.FirstOrDefault(a =>
                a.HomeAccountId.Identifier == account.RefreshToken);

            if (msalAccount == null)
            {
                _logger.LogWarning("No MSAL account found for refresh");
                return false;
            }

            var result = await _msalClient
                .AcquireTokenSilent(Scopes, msalAccount)
                .ExecuteAsync();

            // Repeat Xbox Live -> XSTS -> Minecraft flow
            var xboxToken = await AuthenticateXboxLiveAsync(result.AccessToken);
            if (xboxToken == null) return false;

            var xstsToken = await AuthenticateXstsAsync(xboxToken);
            if (xstsToken == null) return false;

            var minecraftToken = await AuthenticateMinecraftAsync(xstsToken.Token, xstsToken.UserHash);
            if (minecraftToken == null) return false;

            account.AccessToken = minecraftToken;
            account.TokenExpiresAt = result.ExpiresOn.UtcDateTime;

            _logger.LogInformation("Token refreshed successfully for {Username}", account.Username);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh token for {Username}", account.Username);
            return false;
        }
    }

    public async Task<UserAccount?> GetMinecraftProfileAsync(string accessToken)
    {
        try
        {
            _logger.LogInformation("Fetching Minecraft profile");

            using var request = new HttpRequestMessage(HttpMethod.Get, MinecraftProfileUrl);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get Minecraft profile: {StatusCode}", response.StatusCode);
                return null;
            }

            var profile = await response.Content.ReadFromJsonAsync<MinecraftProfileResponse>();
            if (profile == null)
            {
                return null;
            }

            return new UserAccount
            {
                Username = profile.Name,
                Uuid = profile.Id,
                AvatarUrl = $"https://crafatar.com/avatars/{profile.Id}?overlay"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Minecraft profile");
            return null;
        }
    }

    public async Task SignOutAsync(UserAccount account)
    {
        try
        {
            _logger.LogInformation("Signing out {Username}", account.Username);

            var accounts = await _msalClient.GetAccountsAsync();
            var msalAccount = accounts.FirstOrDefault(a =>
                a.HomeAccountId.Identifier == account.RefreshToken);

            if (msalAccount != null)
            {
                await _msalClient.RemoveAsync(msalAccount);
            }

            IsAuthenticated = false;
            _logger.LogInformation("Sign out completed for {Username}", account.Username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sign out {Username}", account.Username);
        }
    }

    private async Task<string?> AuthenticateXboxLiveAsync(string msAccessToken)
    {
        try
        {
            var payload = new
            {
                Properties = new
                {
                    AuthMethod = "RPS",
                    SiteName = "user.auth.xboxlive.com",
                    RpsTicket = $"d={msAccessToken}"
                },
                RelyingParty = "http://auth.xboxlive.com",
                TokenType = "JWT"
            };

            var response = await _httpClient.PostAsJsonAsync(XboxAuthUrl, payload);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<XboxLiveResponse>();
            return result?.Token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xbox Live authentication failed");
            return null;
        }
    }

    private async Task<XstsToken?> AuthenticateXstsAsync(string xboxToken)
    {
        try
        {
            var payload = new
            {
                Properties = new
                {
                    SandboxId = "RETAIL",
                    UserTokens = new[] { xboxToken }
                },
                RelyingParty = "rp://api.minecraftservices.com/",
                TokenType = "JWT"
            };

            var response = await _httpClient.PostAsJsonAsync(XstsAuthUrl, payload);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<XstsResponse>();
            if (result == null) return null;

            return new XstsToken
            {
                Token = result.Token,
                UserHash = result.DisplayClaims?.Xui?.FirstOrDefault()?.Uhs ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "XSTS authentication failed");
            return null;
        }
    }

    private async Task<string?> AuthenticateMinecraftAsync(string xstsToken, string userHash)
    {
        try
        {
            var payload = new
            {
                identityToken = $"XBL3.0 x={userHash};{xstsToken}"
            };

            var response = await _httpClient.PostAsJsonAsync(MinecraftAuthUrl, payload);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<MinecraftAuthResponse>();
            return result?.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Minecraft authentication failed");
            return null;
        }
    }

    // Response models
    private class XboxLiveResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    private class XstsResponse
    {
        public string Token { get; set; } = string.Empty;
        public DisplayClaims? DisplayClaims { get; set; }
    }

    private class DisplayClaims
    {
        public List<XuiClaim>? Xui { get; set; }
    }

    private class XuiClaim
    {
        public string Uhs { get; set; } = string.Empty;
    }

    private class XstsToken
    {
        public string Token { get; set; } = string.Empty;
        public string UserHash { get; set; } = string.Empty;
    }

    private class MinecraftAuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    private class MinecraftProfileResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
