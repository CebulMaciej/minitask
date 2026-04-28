using MediatR;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Application.Common.Mappings;
using FitPlan.Application.Common.Models;
using FitPlan.Domain.ValueObjects;

namespace FitPlan.Application.Sessions.Commands;

public record UpdateSessionCommand(
    string TrainerId, string SessionId, string ClientId,
    DateTime? ScheduledAt, IReadOnlyList<ExerciseInput>? Exercises) : IRequest<SessionDetailDto>, ITenantScopedRequest;

public class UpdateSessionHandler(ISessionRepository sessionRepo) : IRequestHandler<UpdateSessionCommand, SessionDetailDto>
{
    public async Task<SessionDetailDto> Handle(UpdateSessionCommand request, CancellationToken ct)
    {
        var session = await sessionRepo.GetByIdAndTrainerAsync(request.SessionId, request.TrainerId, ct)
            ?? throw new NotFoundException("Session", request.SessionId);

        var scheduledAt = request.ScheduledAt ?? session.ScheduledAt;
        var exercises = request.Exercises != null
            ? request.Exercises.Select((e, i) => new Exercise(e.Name, i, e.Sets, e.Reps, e.TargetWeight)).ToList()
            : session.Exercises.ToList();

        session.Update(scheduledAt, exercises);
        await sessionRepo.UpdateAsync(session, ct);
        return session.ToDetailDto();
    }
}
