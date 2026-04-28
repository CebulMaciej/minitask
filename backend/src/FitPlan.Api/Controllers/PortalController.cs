using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Application.Portal.Queries;

namespace FitPlan.Api.Controllers;

[ApiController]
[Route("api/portal")]
[Authorize]
public class PortalController(IMediator mediator, ICurrentTrainerAccessor accessor) : ControllerBase
{
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions(
        [FromQuery] int page = 1, [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        if (!accessor.IsClient) return Forbid();
        var result = await mediator.Send(new GetPortalSessionsQuery(accessor.UserId!, page, limit), ct);
        return Ok(result);
    }

    [HttpGet("sessions/{sessionId}")]
    public async Task<IActionResult> GetSession(string sessionId, CancellationToken ct)
    {
        if (!accessor.IsClient) return Forbid();
        var result = await mediator.Send(new GetPortalSessionDetailQuery(accessor.UserId!, sessionId), ct);
        return Ok(result);
    }
}
