using Microsoft.Extensions.Configuration;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Domain.Entities;
using FitPlan.Domain.Enums;

namespace FitPlan.Infrastructure.Services;

public class RefreshTokenService(IRefreshTokenRepository tokenRepo, IConfiguration config) : IRefreshTokenService
{
    private readonly int _expiryDays = int.TryParse(config["Jwt:RefreshTokenExpiryDays"], out var d) ? d : 7;

    public async Task<string> GenerateAndStoreAsync(string userId, UserType userType, CancellationToken ct = default)
    {
        var token = new RefreshToken(userId, userType, Guid.NewGuid().ToString("N"), DateTime.UtcNow.AddDays(_expiryDays));
        await tokenRepo.InsertAsync(token, ct);
        return token.Token;
    }

    public async Task<(string userId, UserType userType)?> RotateAsync(string token, CancellationToken ct = default)
    {
        var existing = await tokenRepo.GetActiveByTokenAsync(token, ct);
        if (existing == null) return null;

        existing.Revoke();
        await tokenRepo.UpdateAsync(existing, ct);

        return (existing.UserId, existing.UserType);
    }

    public async Task RevokeAsync(string token, CancellationToken ct = default)
    {
        var existing = await tokenRepo.GetActiveByTokenAsync(token, ct);
        if (existing == null) return;
        existing.Revoke();
        await tokenRepo.UpdateAsync(existing, ct);
    }
}
