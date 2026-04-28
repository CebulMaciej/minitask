using MediatR;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;

namespace FitPlan.Application.Clients.Commands;

public record RemoveClientCommand(string TrainerId, string ClientId) : IRequest, ITenantScopedRequest;

public class RemoveClientHandler(IClientRepository clientRepo) : IRequestHandler<RemoveClientCommand>
{
    public async Task Handle(RemoveClientCommand request, CancellationToken ct)
    {
        var client = await clientRepo.GetByIdAndTrainerAsync(request.ClientId, request.TrainerId, ct)
            ?? throw new NotFoundException("Client", request.ClientId);
        await clientRepo.DeleteAsync(client.Id, ct);
    }
}
