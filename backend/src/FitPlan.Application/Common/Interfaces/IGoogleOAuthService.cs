namespace FitPlan.Application.Common.Interfaces;

public record GoogleUserInfo(string GoogleId, string Email, string Name);

public interface IGoogleOAuthService
{
    string BuildAuthorizationUrl(string state);
    Task<GoogleUserInfo?> ExchangeCodeAsync(string code, CancellationToken ct = default);
}
