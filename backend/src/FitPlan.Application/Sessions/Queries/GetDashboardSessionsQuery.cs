using MediatR;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Application.Common.Mappings;
using FitPlan.Application.Common.Models;

namespace FitPlan.Application.Sessions.Queries;

public record GetDashboardSessionsQuery(string TrainerId, DateTime? From, DateTime? To)
    : IRequest<IReadOnlyList<DashboardSessionDto>>;

public class GetDashboardSessionsHandler(ISessionRepository sessionRepo, IClientRepository clientRepo)
    : IRequestHandler<GetDashboardSessionsQuery, IReadOnlyList<DashboardSessionDto>>
{
    public async Task<IReadOnlyList<DashboardSessionDto>> Handle(GetDashboardSessionsQuery request, CancellationToken ct)
    {
        var sessions = await sessionRepo.GetByTrainerAsync(request.TrainerId, request.From, request.To, ct);
        var clients = await clientRepo.GetByTrainerAsync(request.TrainerId, ct);
        var clientMap = clients.ToDictionary(c => c.Id, c => c.Name);

        return sessions.Select(s => new DashboardSessionDto(
            s.Id, s.ClientId,
            clientMap.TryGetValue(s.ClientId, out var name) ? name : "Unknown",
            s.ScheduledAt, s.Status.ToStatusString(), s.Exercises.Count, s.CompletedAt
        )).ToList();
    }
}
