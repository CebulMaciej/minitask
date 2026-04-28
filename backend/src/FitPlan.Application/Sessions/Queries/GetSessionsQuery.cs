using MediatR;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Application.Common.Mappings;
using FitPlan.Application.Common.Models;
using FitPlan.Domain.Enums;

namespace FitPlan.Application.Sessions.Queries;

public record GetSessionsQuery(
    string TrainerId, string ClientId,
    DateTime? From = null, DateTime? To = null, SessionStatus? Status = null)
    : IRequest<IReadOnlyList<SessionSummaryDto>>, ITenantScopedRequest;

public class GetSessionsHandler(ISessionRepository sessionRepo, IClientRepository clientRepo)
    : IRequestHandler<GetSessionsQuery, IReadOnlyList<SessionSummaryDto>>
{
    public async Task<IReadOnlyList<SessionSummaryDto>> Handle(GetSessionsQuery request, CancellationToken ct)
    {
        _ = await clientRepo.GetByIdAndTrainerAsync(request.ClientId, request.TrainerId, ct)
            ?? throw new NotFoundException("Client", request.ClientId);

        var sessions = await sessionRepo.GetByClientAsync(
            request.ClientId, request.TrainerId, request.From, request.To, request.Status, ct);

        return sessions.Select(s => new SessionSummaryDto(
            s.Id, s.ScheduledAt, s.Status.ToString(), s.Exercises.Count, s.CompletedAt)).ToList();
    }
}

public record GetSessionQuery(string TrainerId, string SessionId) : IRequest<SessionDetailDto>, ITenantScopedRequest;

public class GetSessionHandler(ISessionRepository sessionRepo) : IRequestHandler<GetSessionQuery, SessionDetailDto>
{
    public async Task<SessionDetailDto> Handle(GetSessionQuery request, CancellationToken ct)
    {
        var session = await sessionRepo.GetByIdAndTrainerAsync(request.SessionId, request.TrainerId, ct)
            ?? throw new NotFoundException("Session", request.SessionId);
        return session.ToDetailDto();
    }
}
