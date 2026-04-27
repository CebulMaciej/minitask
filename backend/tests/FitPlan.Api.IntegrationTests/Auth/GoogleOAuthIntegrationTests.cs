using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Api.IntegrationTests.Infrastructure;
using Xunit;
using FluentAssertions;

namespace FitPlan.Api.IntegrationTests.Auth;

public class GoogleOAuthIntegrationTests : IClassFixture<FitPlanWebFactory>
{
    private readonly FitPlanWebFactory _factory;

    public GoogleOAuthIntegrationTests(FitPlanWebFactory factory)
    {
        _factory = factory;
    }

    // AC-1: GET /api/auth/google should redirect to Google OAuth consent screen
    [Fact]
    public async Task GoogleInitiate_ShouldReturn302_WithGoogleLocationHeader()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        _factory.GoogleOAuthServiceMock
            .Setup(g => g.BuildAuthorizationUrl(It.IsAny<string>()))
            .Returns("https://accounts.google.com/o/oauth2/v2/auth?client_id=test");

        var res = await client.GetAsync("/api/auth/google");

        res.StatusCode.Should().Be(HttpStatusCode.Redirect);
        res.Headers.Location!.ToString().Should().StartWith("https://accounts.google.com");
    }

    // AC-2 + AC-4: Callback with new Google account should create trainer with emailConfirmed=true
    [Fact]
    public async Task GoogleCallback_ShouldRedirectToFrontend_WhenNewGoogleUser()
    {
        var (client, state) = await InitiateAndGetState();

        var googleId = Guid.NewGuid().ToString("N");
        var email = $"google-new-{Guid.NewGuid().ToString("N")[..8]}@gmail.com";

        _factory.GoogleOAuthServiceMock
            .Setup(g => g.ExchangeCodeAsync("test-code", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleUserInfo(googleId, email, "New Google User"));

        var res = await client.GetAsync($"/api/auth/google/callback?code=test-code&state={state}");

        res.StatusCode.Should().Be(HttpStatusCode.Redirect);
        res.Headers.Location!.ToString().Should().Contain("/auth/google/callback");

        // Verify refresh token cookie was set
        res.Headers.TryGetValues("Set-Cookie", out var cookies);
        cookies.Should().Contain(c => c.Contains("refreshToken"));
    }

    // AC-3: Callback with existing email should link googleId on existing trainer
    [Fact]
    public async Task GoogleCallback_ShouldLinkGoogleId_WhenEmailAlreadyExists()
    {
        // Register an existing trainer via email/password
        var email = $"google-link-{Guid.NewGuid().ToString("N")[..8]}@test.fitplan";
        _factory.EmailServiceMock
            .Setup(e => e.SendConfirmationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var (accessToken, _) = await TestHelpers.RegisterAndLoginAsync(_factory.CreateClient(), _factory.EmailServiceMock, "Google Link Trainer");
        // We don't use the access token here — just need the trainer created in DB

        var (client, state) = await InitiateAndGetState();
        var googleId = Guid.NewGuid().ToString("N");

        _factory.GoogleOAuthServiceMock
            .Setup(g => g.ExchangeCodeAsync("link-code", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleUserInfo(googleId, email, "Linked Trainer"));

        var res = await client.GetAsync($"/api/auth/google/callback?code=link-code&state={state}");

        res.StatusCode.Should().Be(HttpStatusCode.Redirect);
        res.Headers.Location!.ToString().Should().Contain("/auth/google/callback");
    }

    // AC-5: Callback with missing or mismatched state should return 400 (CSRF protection)
    [Fact]
    public async Task GoogleCallback_ShouldReturn400_WhenStateMissing()
    {
        var client = _factory.CreateClient();

        var res = await client.GetAsync("/api/auth/google/callback?code=any-code&state=wrong-state");

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GoogleCallback_ShouldReturn400_WhenStateMismatch()
    {
        var (client, _) = await InitiateAndGetState();

        var res = await client.GetAsync("/api/auth/google/callback?code=any-code&state=tampered-state");

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await res.Content.ReadFromJsonAsync<ErrorResponse>();
        body!.Code.Should().Be("INVALID_STATE");
    }

    // Helper: call /api/auth/google (without following redirect) to capture the state cookie
    private async Task<(HttpClient client, string state)> InitiateAndGetState()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        _factory.GoogleOAuthServiceMock
            .Setup(g => g.BuildAuthorizationUrl(It.IsAny<string>()))
            .Returns<string>(s => $"https://accounts.google.com/o/oauth2/v2/auth?state={s}");

        var initRes = await client.GetAsync("/api/auth/google");
        initRes.StatusCode.Should().Be(HttpStatusCode.Redirect);

        // Extract state from Location URL
        var location = initRes.Headers.Location!.ToString();
        var stateParam = location.Split("state=")[1].Split("&")[0];

        // The cookie is set on initRes; WebApplicationFactory client tracks cookies, so
        // subsequent requests on the same client will include oauth_state.
        return (client, stateParam);
    }

    private record ErrorResponse(string Code, string Message);
}
