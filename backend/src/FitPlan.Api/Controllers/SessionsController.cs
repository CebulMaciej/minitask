using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Application.Sessions.Commands;
using FitPlan.Application.Sessions.Queries;
using FitPlan.Domain.Enums;

namespace FitPlan.Api.Controllers;

[ApiController]
[Route("api/clients/{clientId}/sessions")]
[Authorize]
public class SessionsController(IMediator mediator, ICurrentTrainerAccessor accessor) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSessions(
        string clientId,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        if (!accessor.IsTrainer) return Forbid();
        SessionStatus? statusEnum = status != null && Enum.TryParse<SessionStatus>(status, true, out var s) ? s : null;
        var result = await mediator.Send(new GetSessionsQuery(accessor.TrainerId!, clientId, from, to, statusEnum), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSession(string clientId, [FromBody] CreateSessionRequest req, CancellationToken ct)
    {
        if (!accessor.IsTrainer) return Forbid();
        var result = await mediator.Send(new CreateSessionCommand(
            accessor.TrainerId!, clientId, req.ScheduledAt,
            req.Exercises.Select(e => new ExerciseInput(e.Name, e.Sets, e.Reps, e.TargetWeight)).ToList()), ct);
        return StatusCode(201, result);
    }

    [HttpGet("{sessionId}")]
    public async Task<IActionResult> GetSession(string clientId, string sessionId, CancellationToken ct)
    {
        if (!accessor.IsTrainer) return Forbid();
        var result = await mediator.Send(new GetSessionQuery(accessor.TrainerId!, sessionId), ct);
        return Ok(result);
    }

    [HttpPut("{sessionId}")]
    public async Task<IActionResult> UpdateSession(string clientId, string sessionId, [FromBody] UpdateSessionRequest req, CancellationToken ct)
    {
        if (!accessor.IsTrainer) return Forbid();
        var exercises = req.Exercises?.Select(e => new ExerciseInput(e.Name, e.Sets, e.Reps, e.TargetWeight)).ToList();
        var result = await mediator.Send(new UpdateSessionCommand(accessor.TrainerId!, sessionId, clientId, req.ScheduledAt, exercises), ct);
        return Ok(result);
    }

    [HttpDelete("{sessionId}")]
    public async Task<IActionResult> DeleteSession(string clientId, string sessionId, CancellationToken ct)
    {
        if (!accessor.IsTrainer) return Forbid();
        await mediator.Send(new DeleteSessionCommand(accessor.TrainerId!, sessionId), ct);
        return NoContent();
    }

    [HttpPost("{sessionId}/start")]
    public async Task<IActionResult> StartSession(string clientId, string sessionId, CancellationToken ct)
    {
        if (!accessor.IsTrainer) return Forbid();
        var result = await mediator.Send(new StartSessionCommand(accessor.TrainerId!, sessionId), ct);
        return Ok(result);
    }

    [HttpPatch("{sessionId}/exercises/{exerciseId}")]
    public async Task<IActionResult> LogActuals(
        string clientId, string sessionId, string exerciseId,
        [FromBody] LogActualsRequest req, CancellationToken ct)
    {
        if (!accessor.IsTrainer) return Forbid();
        var result = await mediator.Send(new LogExerciseActualsCommand(
            accessor.TrainerId!, sessionId, exerciseId,
            req.ActualSets, req.ActualReps, req.ActualWeight, req.Notes), ct);
        return Ok(result);
    }

    [HttpPost("{sessionId}/complete")]
    public async Task<IActionResult> CompleteSession(string clientId, string sessionId, CancellationToken ct)
    {
        if (!accessor.IsTrainer) return Forbid();
        var result = await mediator.Send(new CompleteSessionCommand(accessor.TrainerId!, sessionId), ct);
        return Ok(result);
    }
}

public record ExerciseInputRequest(string Name, int Sets, int Reps, double? TargetWeight);
public record CreateSessionRequest(DateTime ScheduledAt, List<ExerciseInputRequest> Exercises);
public record UpdateSessionRequest(DateTime? ScheduledAt, List<ExerciseInputRequest>? Exercises);
public record LogActualsRequest(int? ActualSets, int? ActualReps, double? ActualWeight, string? Notes);
