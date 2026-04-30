using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using FitPlan.Application.Common.Interfaces;

namespace FitPlan.Infrastructure.Services;

public class EmailService(IConfiguration config, ILogger<EmailService> logger) : IEmailService
{
    private readonly string _host = config["Smtp:Host"] ?? "localhost";
    private readonly int _port = int.TryParse(config["Smtp:Port"], out var p) ? p : 1025;
    private readonly string? _username = config["Smtp:Username"];
    private readonly string? _password = config["Smtp:Password"];
    private readonly string _from = config["Smtp:From"] ?? "noreply@fitplan.app";
    private readonly string _frontendUrl = config["FrontendUrl"] ?? "http://localhost:5173";

    public Task SendConfirmationEmailAsync(string toEmail, string toName, string token, CancellationToken ct = default)
    {
        var link = $"{_frontendUrl}/confirm-email?token={Uri.EscapeDataString(token)}";
        return SendAsync(toEmail, toName, "Confirm your FitPlan email",
            $"<p>Hi {toName},</p><p>Please confirm your email by clicking the link below:</p><p><a href=\"{link}\">Confirm Email</a></p><p>Or copy this link: {link}</p>", ct);
    }

    public Task SendClientInvitationAsync(string toEmail, string toName, string trainerName, string tempPassword, CancellationToken ct = default)
        => SendAsync(toEmail, toName, $"You've been invited to FitPlan by {trainerName}",
            $"<p>Hi {toName},</p><p>Your trainer <strong>{trainerName}</strong> has added you to FitPlan.</p><p>Login at: <a href=\"{_frontendUrl}/login\">{_frontendUrl}/login</a></p><p>Temporary password: <strong>{tempPassword}</strong></p>", ct);

    public Task SendPasswordResetEmailAsync(string toEmail, string toName, string token, CancellationToken ct = default)
    {
        var link = $"{_frontendUrl}/reset-password?token={Uri.EscapeDataString(token)}";
        return SendAsync(toEmail, toName, "Reset your FitPlan password",
            $"<p>Hi {toName},</p><p>Reset your password by clicking the link below:</p><p><a href=\"{link}\">Reset Password</a></p><p>Or copy this link: {link}</p>", ct);
    }

    private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_from));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(_host, _port, SecureSocketOptions.None, ct);
            if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
                await client.AuthenticateAsync(_username, _password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send email to {Email}", toEmail);
        }
    }
}
