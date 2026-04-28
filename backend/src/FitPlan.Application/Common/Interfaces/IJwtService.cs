using FitPlan.Domain.Enums;

namespace FitPlan.Application.Common.Interfaces;

public record JwtClaims(string UserId, UserType UserType, string? TrainerId);

public interface IJwtService
{
    string GenerateAccessToken(string userId, UserType userType, string? trainerId = null);
    JwtClaims? ValidateAccessToken(string token);
}
