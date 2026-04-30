using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitPlan.Application.Auth.Commands;
using FitPlan.Application.Auth.Queries;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Domain.Enums;

namespace FitPlan.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator, IGoogleOAuthService googleOAuth, IConfiguration config) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        await mediator.Send(new RegisterTrainerCommand(req.Name, req.Email, req.Password), ct);
        return StatusCode(201, new { message = "Registration successful. Please confirm your email." });
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest req, CancellationToken ct)
    {
        await mediator.Send(new ConfirmEmailCommand(req.Token), ct);
        return Ok(new { message = "Email confirmed successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var userType = Enum.Parse<UserType>(req.UserType, ignoreCase: true);
        var result = await mediator.Send(new LoginQuery(req.Email, req.Password, userType), ct);

        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(new { accessToken = result.AccessToken, userType = result.UserType.ToString() });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var token = Request.Cookies["refreshToken"];
        if (token is null)
            return Unauthorized(new { code = "UNAUTHORIZED", message = "Refresh token missing." });

        var result = await mediator.Send(new RefreshTokenCommand(token), ct);
        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(new { accessToken = result.AccessToken, userType = result.UserType.ToString() });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var token = Request.Cookies["refreshToken"];
        if (token != null) await mediator.Send(new LogoutCommand(token), ct);
        Response.Cookies.Delete("refreshToken");
        return NoContent();
    }

    [HttpGet("google")]
    public IActionResult GoogleOAuthInitiate()
    {
        var state = Guid.NewGuid().ToString("N");
        Response.Cookies.Append("oauth_state", state, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromMinutes(10)
        });
        return Redirect(googleOAuth.BuildAuthorizationUrl(state));
    }

    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleOAuthCallback(
        [FromQuery] string code,
        [FromQuery] string state,
        CancellationToken ct)
    {
        var storedState = Request.Cookies["oauth_state"];
        if (string.IsNullOrEmpty(storedState) || storedState != state)
            return BadRequest(new { code = "INVALID_STATE", message = "Invalid OAuth state parameter." });

        Response.Cookies.Delete("oauth_state");

        var result = await mediator.Send(new GoogleOAuthCallbackCommand(code), ct);
        SetRefreshTokenCookie(result.RefreshToken);

        var frontendUrl = config["FrontendUrl"] ?? "http://localhost:5173";
        return Redirect($"{frontendUrl}/auth/google/callback");
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req, CancellationToken ct)
    {
        await mediator.Send(new ForgotPasswordCommand(req.Email), ct);
        return Ok(new { message = "If this email exists, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req, CancellationToken ct)
    {
        await mediator.Send(new ResetPasswordCommand(req.Token, req.NewPassword), ct);
        return Ok(new { message = "Password reset successfully." });
    }

    private void SetRefreshTokenCookie(string token)
    {
        Response.Cookies.Append("refreshToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }
}

public record RegisterRequest(string Name, string Email, string Password);
public record ConfirmEmailRequest(string Token);
public record LoginRequest(string Email, string Password, string UserType);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Token, string NewPassword);
