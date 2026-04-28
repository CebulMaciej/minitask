using MediatR;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Domain.Enums;

namespace FitPlan.Application.Sessions.Commands;

public record DeleteSessionCommand(string TrainerId, string SessionId) : IRequest, ITenantScopedRequest;

public class DeleteSessionHandler(ISessionRepository sessionRepo) : IRequestHandler<DeleteSessionCommand>
{
    public async Task Handle(DeleteSessionCommand request, CancellationToken ct)
    {
        var session = await sessionRepo.GetByIdAndTrainerAsync(request.SessionId, request.TrainerId, ct)
            ?? throw new NotFoundException("Session", request.SessionId);

        if (session.Status == SessionStatus.InProgress)
            throw new FitPlan.Domain.Exceptions.DomainException("Cannot delete an in-progress session.");

        await sessionRepo.DeleteAsync(session.Id, ct);
    }
}
