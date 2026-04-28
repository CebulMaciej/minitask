using System.Net.Http.Json;
using Moq;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Domain.Entities;

namespace FitPlan.Api.IntegrationTests.Infrastructure;

public static class TestHelpers
{
    public static async Task<(string accessToken, string email)> RegisterAndLoginAsync(
        HttpClient client, Mock<IEmailService> emailMock, string name = "Test Trainer")
    {
        var email = $"trainer-{Guid.NewGuid().ToString("N")[..8]}@test.fitplan";
        const string password = "TestPassword1!";

        // Capture confirmation token from email service call
        string? confirmationToken = null;
        emailMock
            .Setup(e => e.SendConfirmationEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, string, CancellationToken>((_, _, token, _) =>
                confirmationToken = token)
            .Returns(Task.CompletedTask);

        // Register
        var registerRes = await client.PostAsJsonAsync("/api/auth/register",
            new { name, email, password });
        registerRes.EnsureSuccessStatusCode();

        // Confirm email
        var confirmRes = await client.PostAsJsonAsync("/api/auth/confirm-email",
            new { token = confirmationToken! });
        confirmRes.EnsureSuccessStatusCode();

        // Login
        var loginRes = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password, userType = "TRAINER" });
        loginRes.EnsureSuccessStatusCode();

        var loginBody = await loginRes.Content.ReadFromJsonAsync<LoginResponse>();
        return (loginBody!.AccessToken, email);
    }

    public record LoginResponse(string AccessToken, string UserType);
}
