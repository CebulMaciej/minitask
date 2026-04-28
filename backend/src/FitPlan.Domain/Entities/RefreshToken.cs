using FitPlan.Domain.Common;
using FitPlan.Domain.Enums;

namespace FitPlan.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string UserId { get; private set; }
    public UserType UserType { get; private set; }
    public string Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    private RefreshToken() { UserId = string.Empty; Token = string.Empty; }

    public RefreshToken(string userId, UserType userType, string token, DateTime expiresAt)
    {
        UserId = userId;
        UserType = userType;
        Token = token;
        ExpiresAt = expiresAt;
    }

    public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
        Touch();
    }
}
