using MediatR;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Application.Common.Mappings;
using FitPlan.Application.Common.Models;

namespace FitPlan.Application.Portal.Queries;

public record GetPortalSessionsQuery(string ClientId, int Page = 1, int Limit = 20)
    : IRequest<PaginatedResult<SessionSummaryDto>>;

public class GetPortalSessionsHandler(ISessionRepository sessionRepo)
    : IRequestHandler<GetPortalSessionsQuery, PaginatedResult<SessionSummaryDto>>
{
    public async Task<PaginatedResult<SessionSummaryDto>> Handle(GetPortalSessionsQuery request, CancellationToken ct)
    {
        var sessions = await sessionRepo.GetCompletedByClientAsync(request.ClientId, request.Page, request.Limit, ct);
        var total = await sessionRepo.CountCompletedByClientAsync(request.ClientId, ct);

        var items = sessions.Select(s => new SessionSummaryDto(
            s.Id, s.ScheduledAt, s.Status.ToString(), s.Exercises.Count, s.CompletedAt)).ToList();

        return new PaginatedResult<SessionSummaryDto>(items, total, request.Page, request.Limit);
    }
}

public record GetPortalSessionDetailQuery(string ClientId, string SessionId) : IRequest<SessionDetailDto>;

public class GetPortalSessionDetailHandler(ISessionRepository sessionRepo)
    : IRequestHandler<GetPortalSessionDetailQuery, SessionDetailDto>
{
    public async Task<SessionDetailDto> Handle(GetPortalSessionDetailQuery request, CancellationToken ct)
    {
        var session = await sessionRepo.GetByIdAndClientAsync(request.SessionId, request.ClientId, ct)
            ?? throw new NotFoundException("Session", request.SessionId);
        return session.ToDetailDto();
    }
}
