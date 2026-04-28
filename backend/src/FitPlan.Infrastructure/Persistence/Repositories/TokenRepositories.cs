using FitPlan.Application.Common.Interfaces;
using FitPlan.Domain.Entities;

namespace FitPlan.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository(IMongoContext context)
    : MongoRepository<RefreshToken>(context, "refresh_tokens"), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetActiveByTokenAsync(string token, CancellationToken ct = default)
        => await FindOneAsync(t => t.Token == token && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow, ct);

    public async Task RevokeAllForUserAsync(string userId, CancellationToken ct = default)
    {
        var tokens = await FindAsync(t => t.UserId == userId && t.RevokedAt == null, ct);
        foreach (var t in tokens)
        {
            t.Revoke();
            await UpdateAsync(t, ct);
        }
    }
}

public class EmailTokenRepository(IMongoContext context)
    : MongoRepository<EmailConfirmationToken>(context, "email_confirmation_tokens"), IEmailTokenRepository
{
    public async Task<EmailConfirmationToken?> GetValidByTokenAsync(string token, CancellationToken ct = default)
        => await FindOneAsync(t => t.Token == token && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow, ct);
}
