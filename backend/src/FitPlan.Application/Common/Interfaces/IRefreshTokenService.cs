using FitPlan.Domain.Enums;

namespace FitPlan.Application.Common.Interfaces;

public interface IRefreshTokenService
{
    Task<string> GenerateAndStoreAsync(string userId, UserType userType, CancellationToken ct = default);
    Task<(string userId, UserType userType)?> RotateAsync(string token, CancellationToken ct = default);
    Task RevokeAsync(string token, CancellationToken ct = default);
}
