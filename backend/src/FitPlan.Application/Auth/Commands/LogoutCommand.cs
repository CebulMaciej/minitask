using MediatR;
using FitPlan.Application.Common.Interfaces;

namespace FitPlan.Application.Auth.Commands;

public record LogoutCommand(string Token) : IRequest;

public class LogoutHandler(IRefreshTokenService refreshTokenService) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken ct)
        => await refreshTokenService.RevokeAsync(request.Token, ct);
}
