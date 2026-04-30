using MediatR;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Application.Common.Models;

namespace FitPlan.Application.Clients.Queries;

public record GetClientsQuery(string TrainerId) : IRequest<IReadOnlyList<ClientSummaryDto>>, ITenantScopedRequest;

public class GetClientsHandler(IClientRepository clientRepo, ISessionRepository sessionRepo)
    : IRequestHandler<GetClientsQuery, IReadOnlyList<ClientSummaryDto>>
{
    public async Task<IReadOnlyList<ClientSummaryDto>> Handle(GetClientsQuery request, CancellationToken ct)
    {
        var clients = await clientRepo.GetByTrainerAsync(request.TrainerId, ct);
        var lastSessionTasks = clients.Select(c => sessionRepo.GetLastCompletedAtByClientAsync(c.Id, ct));
        var lastSessions = await Task.WhenAll(lastSessionTasks);
        return clients.Select((c, i) => new ClientSummaryDto(c.Id, c.Name, c.Email, c.CreatedAt, lastSessions[i])).ToList();
    }
}

public record GetClientQuery(string TrainerId, string ClientId) : IRequest<ClientDetailDto>, ITenantScopedRequest;

public class GetClientHandler(IClientRepository clientRepo, ISessionRepository sessionRepo) : IRequestHandler<GetClientQuery, ClientDetailDto>
{
    public async Task<ClientDetailDto> Handle(GetClientQuery request, CancellationToken ct)
    {
        var client = await clientRepo.GetByIdAndTrainerAsync(request.ClientId, request.TrainerId, ct)
            ?? throw new NotFoundException("Client", request.ClientId);

        var totalSessions = await sessionRepo.CountCompletedByClientAsync(client.Id, ct);
        var sessions = await sessionRepo.GetCompletedByClientAsync(client.Id, 1, 1, ct);
        var lastSession = sessions.FirstOrDefault()?.CompletedAt;

        return new ClientDetailDto(client.Id, client.Name, client.Email, client.CreatedAt, totalSessions, lastSession);
    }
}
