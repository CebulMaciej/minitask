using FitPlan.Domain.Entities;
using FitPlan.Domain.Enums;

namespace FitPlan.Application.Common.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetActiveByTokenAsync(string token, CancellationToken ct = default);
    Task RevokeAllForUserAsync(string userId, CancellationToken ct = default);
}

public interface IEmailTokenRepository : IRepository<EmailConfirmationToken>
{
    Task<EmailConfirmationToken?> GetValidByTokenAsync(string token, CancellationToken ct = default);
}
