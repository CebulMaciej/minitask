using FitPlan.Domain.Common;

namespace FitPlan.Domain.Entities;

public class Trainer : BaseEntity
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string? PasswordHash { get; private set; }
    public string? GoogleId { get; private set; }
    public bool EmailConfirmed { get; private set; }

    private Trainer() { Name = string.Empty; Email = string.Empty; }

    public Trainer(string name, string email, string? passwordHash = null, string? googleId = null)
    {
        Name = name;
        Email = email.ToLowerInvariant();
        PasswordHash = passwordHash;
        GoogleId = googleId;
        EmailConfirmed = googleId != null;
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
        Touch();
    }

    public void LinkGoogle(string googleId)
    {
        GoogleId = googleId;
        EmailConfirmed = true;
        Touch();
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        Touch();
    }
}
