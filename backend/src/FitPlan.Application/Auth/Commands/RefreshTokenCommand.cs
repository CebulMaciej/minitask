using MediatR;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Application.Common.Models;

namespace FitPlan.Application.Auth.Commands;

public record RefreshTokenCommand(string Token) : IRequest<AuthResult>;

public class RefreshTokenHandler(
    IJwtService jwtService,
    IRefreshTokenService refreshTokenService,
    IClientRepository clientRepo) : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var (userId, userType) = await refreshTokenService.RotateAsync(request.Token, ct)
            ?? throw new UnauthorizedException("Invalid or expired refresh token.");

        string? trainerId = null;

        if (userType == Domain.Enums.UserType.Client)
        {
            var client = await clientRepo.GetByIdAsync(userId, ct)
                ?? throw new UnauthorizedException();
            trainerId = client.TrainerId;
        }

        var accessToken = jwtService.GenerateAccessToken(userId, userType, trainerId);
        var newRefreshToken = await refreshTokenService.GenerateAndStoreAsync(userId, userType, ct);
        return new AuthResult(accessToken, newRefreshToken, userType);
    }
}
