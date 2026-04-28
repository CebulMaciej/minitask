using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Domain.Enums;

namespace FitPlan.Infrastructure.Services;

public class JwtService(IConfiguration config) : IJwtService
{
    private readonly string _secret = config["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret not configured.");
    private readonly string _issuer = config["Jwt:Issuer"] ?? "fitplan-api";
    private readonly string _audience = config["Jwt:Audience"] ?? "fitplan-client";
    private readonly int _expiryMinutes = int.TryParse(config["Jwt:AccessTokenExpiryMinutes"], out var m) ? m : 15;

    public string GenerateAccessToken(string userId, UserType userType, string? trainerId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new("userType", userType.ToString()),
        };
        if (trainerId != null) claims.Add(new("trainerId", trainerId));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public JwtClaims? ValidateAccessToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));

            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            }, out var validated);

            var jwt = (JwtSecurityToken)validated;
            var userId = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var userTypeStr = jwt.Claims.First(c => c.Type == "userType").Value;
            var trainerId = jwt.Claims.FirstOrDefault(c => c.Type == "trainerId")?.Value;

            var userType = Enum.Parse<UserType>(userTypeStr);
            return new JwtClaims(userId, userType, trainerId);
        }
        catch
        {
            return null;
        }
    }
}
