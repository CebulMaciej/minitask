using MediatR;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Application.Common.Models;
using FitPlan.Domain.Entities;
using FitPlan.Domain.Enums;

namespace FitPlan.Application.Auth.Commands;

public record GoogleOAuthCallbackCommand(string Code) : IRequest<AuthResult>;

public class GoogleOAuthCallbackHandler(
    IGoogleOAuthService googleOAuth,
    ITrainerRepository trainerRepo,
    IJwtService jwtService,
    IRefreshTokenService refreshTokenService) : IRequestHandler<GoogleOAuthCallbackCommand, AuthResult>
{
    public async Task<AuthResult> Handle(GoogleOAuthCallbackCommand request, CancellationToken ct)
    {
        var userInfo = await googleOAuth.ExchangeCodeAsync(request.Code, ct)
            ?? throw new UnauthorizedException("Failed to retrieve Google profile.");

        var trainer = await trainerRepo.GetByGoogleIdAsync(userInfo.GoogleId, ct);

        if (trainer == null)
        {
            trainer = await trainerRepo.GetByEmailAsync(userInfo.Email, ct);
            if (trainer != null)
            {
                trainer.LinkGoogle(userInfo.GoogleId);
                await trainerRepo.UpdateAsync(trainer, ct);
            }
            else
            {
                trainer = new Trainer(userInfo.Name, userInfo.Email, passwordHash: null, googleId: userInfo.GoogleId);
                await trainerRepo.InsertAsync(trainer, ct);
            }
        }

        var accessToken = jwtService.GenerateAccessToken(trainer.Id, UserType.Trainer);
        var refreshToken = await refreshTokenService.GenerateAndStoreAsync(trainer.Id, UserType.Trainer, ct);
        return new AuthResult(accessToken, refreshToken, UserType.Trainer);
    }
}
