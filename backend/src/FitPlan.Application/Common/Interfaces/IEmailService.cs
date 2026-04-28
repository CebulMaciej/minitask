namespace FitPlan.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendConfirmationEmailAsync(string toEmail, string toName, string confirmationToken, CancellationToken ct = default);
    Task SendClientInvitationAsync(string toEmail, string toName, string trainerName, string temporaryPassword, CancellationToken ct = default);
    Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetToken, CancellationToken ct = default);
}
