using FitPlan.Domain.Common;

namespace FitPlan.Domain.Entities;

public class Client : BaseEntity
{
    public string TrainerId { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string? PasswordHash { get; private set; }
    public string? GoogleId { get; private set; }
    public bool EmailConfirmed { get; private set; }

    private Client() { TrainerId = string.Empty; Name = string.Empty; Email = string.Empty; }

    public Client(string trainerId, string name, string email, string? passwordHash = null)
    {
        TrainerId = trainerId;
        Name = name;
        Email = email.ToLowerInvariant();
        PasswordHash = passwordHash;
        EmailConfirmed = false;
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

    public void SetTemporaryPassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        Touch();
    }
}
