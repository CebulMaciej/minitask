using FluentValidation;
using MediatR;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Application.Common.Models;
using FitPlan.Domain.Enums;

namespace FitPlan.Application.Auth.Queries;

public record LoginQuery(string Email, string Password, UserType UserType) : IRequest<AuthResult>;

public class LoginValidator : AbstractValidator<LoginQuery>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginHandler(
    ITrainerRepository trainerRepo,
    IClientRepository clientRepo,
    IJwtService jwtService,
    IRefreshTokenService refreshTokenService) : IRequestHandler<LoginQuery, AuthResult>
{
    public async Task<AuthResult> Handle(LoginQuery request, CancellationToken ct)
    {
        if (request.UserType == UserType.Trainer)
        {
            var trainer = await trainerRepo.GetByEmailAsync(request.Email.ToLowerInvariant(), ct)
                ?? throw new UnauthorizedException("Invalid credentials.");

            if (!trainer.EmailConfirmed)
                throw new UnauthorizedException("Please confirm your email before logging in.");

            if (trainer.PasswordHash == null || !BCrypt.Net.BCrypt.Verify(request.Password, trainer.PasswordHash))
                throw new UnauthorizedException("Invalid credentials.");

            var accessToken = jwtService.GenerateAccessToken(trainer.Id, UserType.Trainer);
            var refreshToken = await refreshTokenService.GenerateAndStoreAsync(trainer.Id, UserType.Trainer, ct);
            return new AuthResult(accessToken, refreshToken, UserType.Trainer);
        }
        else
        {
            var client = await clientRepo.FindOneAsync(
                c => c.Email == request.Email.ToLowerInvariant(), ct)
                ?? throw new UnauthorizedException("Invalid credentials.");

            if (!client.EmailConfirmed)
                throw new UnauthorizedException("Please confirm your email before logging in.");

            if (client.PasswordHash == null || !BCrypt.Net.BCrypt.Verify(request.Password, client.PasswordHash))
                throw new UnauthorizedException("Invalid credentials.");

            var accessToken = jwtService.GenerateAccessToken(client.Id, UserType.Client, client.TrainerId);
            var refreshToken = await refreshTokenService.GenerateAndStoreAsync(client.Id, UserType.Client, ct);
            return new AuthResult(accessToken, refreshToken, UserType.Client);
        }
    }
}
