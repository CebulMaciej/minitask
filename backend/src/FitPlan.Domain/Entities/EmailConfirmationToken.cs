using FitPlan.Domain.Common;
using FitPlan.Domain.Enums;

namespace FitPlan.Domain.Entities;

public class EmailConfirmationToken : BaseEntity
{
    public string UserId { get; private set; }
    public UserType UserType { get; private set; }
    public string Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }

    private EmailConfirmationToken() { UserId = string.Empty; Token = string.Empty; }

    public EmailConfirmationToken(string userId, UserType userType, string token, DateTime expiresAt)
    {
        UserId = userId;
        UserType = userType;
        Token = token;
        ExpiresAt = expiresAt;
    }

    public bool IsValid => UsedAt == null && ExpiresAt > DateTime.UtcNow;

    public void MarkUsed()
    {
        UsedAt = DateTime.UtcNow;
        Touch();
    }
}
