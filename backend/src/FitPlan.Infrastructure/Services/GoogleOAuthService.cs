using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FitPlan.Application.Common.Interfaces;

namespace FitPlan.Infrastructure.Services;

public class GoogleOAuthService(
    HttpClient httpClient,
    IConfiguration config,
    ILogger<GoogleOAuthService> logger) : IGoogleOAuthService
{
    private readonly string _clientId = config["Google:ClientId"] ?? string.Empty;
    private readonly string _clientSecret = config["Google:ClientSecret"] ?? string.Empty;
    private readonly string _redirectUri = config["Google:RedirectUri"] ?? string.Empty;

    public string BuildAuthorizationUrl(string state)
    {
        var query = new Dictionary<string, string>
        {
            ["client_id"] = _clientId,
            ["redirect_uri"] = _redirectUri,
            ["response_type"] = "code",
            ["scope"] = "openid email profile",
            ["state"] = state,
            ["access_type"] = "online"
        };
        var qs = string.Join("&", query.Select(kv =>
            $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
        return $"https://accounts.google.com/o/oauth2/v2/auth?{qs}";
    }

    public async Task<GoogleUserInfo?> ExchangeCodeAsync(string code, CancellationToken ct = default)
    {
        try
        {
            var tokenRes = await httpClient.PostAsync(
                "https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["code"] = code,
                    ["client_id"] = _clientId,
                    ["client_secret"] = _clientSecret,
                    ["redirect_uri"] = _redirectUri,
                    ["grant_type"] = "authorization_code"
                }), ct);

            if (!tokenRes.IsSuccessStatusCode) return null;

            var tokenDoc = JsonDocument.Parse(await tokenRes.Content.ReadAsStringAsync(ct));
            var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();
            if (string.IsNullOrEmpty(accessToken)) return null;

            var userInfoRes = await httpClient.GetAsync(
                $"https://www.googleapis.com/oauth2/v2/userinfo?access_token={Uri.EscapeDataString(accessToken)}", ct);
            if (!userInfoRes.IsSuccessStatusCode) return null;

            var userDoc = JsonDocument.Parse(await userInfoRes.Content.ReadAsStringAsync(ct));
            var root = userDoc.RootElement;

            var googleId = root.GetProperty("id").GetString()!;
            var email = root.GetProperty("email").GetString()!;
            var name = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? email : email;

            return new GoogleUserInfo(googleId, email, name);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Google OAuth token exchange failed");
            return null;
        }
    }
}
