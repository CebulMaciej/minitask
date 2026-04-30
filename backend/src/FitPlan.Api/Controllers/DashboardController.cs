using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Application.Sessions.Queries;

namespace FitPlan.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(IMediator mediator, ICurrentTrainerAccessor accessor) : ControllerBase
{
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        if (!accessor.IsTrainer) return Forbid();
        var result = await mediator.Send(new GetDashboardSessionsQuery(accessor.TrainerId!, from, to), ct);
        return Ok(result);
    }
}
