using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitPlan.Application.Clients.Commands;
using FitPlan.Application.Clients.Queries;
using FitPlan.Application.Common.Interfaces;

namespace FitPlan.Api.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientsController(IMediator mediator, ICurrentTrainerAccessor accessor) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetClients(CancellationToken ct)
    {
        if (!accessor.IsTrainer) return Forbid();
        var result = await mediator.Send(new GetClientsQuery(accessor.TrainerId!), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddClient([FromBody] AddClientRequest req, CancellationToken ct)
    {
        if (!accessor.IsTrainer) return Forbid();
        var result = await mediator.Send(new AddClientCommand(accessor.TrainerId!, req.Name, req.Email), ct);
        return StatusCode(201, result);
    }

    [HttpGet("{clientId}")]
    public async Task<IActionResult> GetClient(string clientId, CancellationToken ct)
    {
        if (!accessor.IsTrainer) return Forbid();
        var result = await mediator.Send(new GetClientQuery(accessor.TrainerId!, clientId), ct);
        return Ok(result);
    }

    [HttpDelete("{clientId}")]
    public async Task<IActionResult> RemoveClient(string clientId, CancellationToken ct)
    {
        if (!accessor.IsTrainer) return Forbid();
        await mediator.Send(new RemoveClientCommand(accessor.TrainerId!, clientId), ct);
        return NoContent();
    }
}

public record AddClientRequest(string Name, string Email);
