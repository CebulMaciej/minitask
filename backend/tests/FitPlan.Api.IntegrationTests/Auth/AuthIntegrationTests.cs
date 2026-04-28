using System.Net;
using System.Net.Http.Json;
using Moq;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Api.IntegrationTests.Infrastructure;
using Xunit;
using FluentAssertions;

namespace FitPlan.Api.IntegrationTests.Auth;

public class AuthIntegrationTests : IClassFixture<FitPlanWebFactory>
{
    private readonly HttpClient _client;
    private readonly FitPlanWebFactory _factory;

    public AuthIntegrationTests(FitPlanWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldReturn201_WithValidData()
    {
        _factory.EmailServiceMock.Setup(e => e.SendConfirmationEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var res = await _client.PostAsJsonAsync("/api/auth/register",
            new { name = "Alice", email = $"alice-{Guid.NewGuid():N}@test.fitplan", password = "SecurePass1!" });

        res.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_ShouldReturn409_WhenEmailAlreadyExists()
    {
        var email = $"dup-{Guid.NewGuid():N}@test.fitplan";
        _factory.EmailServiceMock.Setup(e => e.SendConfirmationEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _client.PostAsJsonAsync("/api/auth/register",
            new { name = "Alice", email, password = "SecurePass1!" });

        var res2 = await _client.PostAsJsonAsync("/api/auth/register",
            new { name = "Alice", email, password = "SecurePass1!" });

        res2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_ShouldReturn401_WithUnconfirmedEmail()
    {
        var email = $"unconf-{Guid.NewGuid():N}@test.fitplan";
        _factory.EmailServiceMock.Setup(e => e.SendConfirmationEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _client.PostAsJsonAsync("/api/auth/register",
            new { name = "Bob", email, password = "SecurePass1!" });

        var loginRes = await _client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "SecurePass1!", userType = "TRAINER" });

        loginRes.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturn200WithAccessToken_WhenEmailConfirmed()
    {
        var (accessToken, _) = await TestHelpers.RegisterAndLoginAsync(
            _client, _factory.EmailServiceMock);

        accessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Refresh_ShouldReturn401_WithoutRefreshTokenCookie()
    {
        // No cookie set — no prior login
        var freshClient = _factory.CreateClient();
        var res = await freshClient.PostAsync("/api/auth/refresh", null);
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ForgotPassword_ShouldAlwaysReturn200_RegardlessOfEmail()
    {
        _factory.EmailServiceMock.Setup(e => e.SendPasswordResetEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var res = await _client.PostAsJsonAsync("/api/auth/forgot-password",
            new { email = "nonexistent@nowhere.com" });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
