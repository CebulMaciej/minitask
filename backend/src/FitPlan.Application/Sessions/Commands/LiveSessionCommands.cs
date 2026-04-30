using MediatR;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Application.Common.Mappings;
using FitPlan.Application.Common.Models;

namespace FitPlan.Application.Sessions.Commands;

public record StartSessionCommand(string TrainerId, string SessionId) : IRequest<SessionDetailDto>;

public class StartSessionHandler(ISessionRepository sessionRepo) : IRequestHandler<StartSessionCommand, SessionDetailDto>
{
    public async Task<SessionDetailDto> Handle(StartSessionCommand request, CancellationToken ct)
    {
        var session = await sessionRepo.GetByIdAndTrainerAsync(request.SessionId, request.TrainerId, ct)
            ?? throw new NotFoundException("Session", request.SessionId);

        session.Start();
        await sessionRepo.UpdateAsync(session, ct);
        return session.ToDetailDto();
    }
}

public record LogExerciseActualsCommand(
    string TrainerId, string SessionId, string ExerciseId,
    int? ActualSets, int? ActualReps, double? ActualWeight, string? Notes) : IRequest<SessionDetailDto>;

public class LogExerciseActualsHandler(ISessionRepository sessionRepo) : IRequestHandler<LogExerciseActualsCommand, SessionDetailDto>
{
    public async Task<SessionDetailDto> Handle(LogExerciseActualsCommand request, CancellationToken ct)
    {
        var session = await sessionRepo.GetByIdAndTrainerAsync(request.SessionId, request.TrainerId, ct)
            ?? throw new NotFoundException("Session", request.SessionId);

        session.LogExerciseActuals(request.ExerciseId, request.ActualSets, request.ActualReps, request.ActualWeight, request.Notes);
        await sessionRepo.UpdateAsync(session, ct);
        return session.ToDetailDto();
    }
}

public record CompleteSessionCommand(string TrainerId, string SessionId) : IRequest<SessionDetailDto>;

public class CompleteSessionHandler(ISessionRepository sessionRepo) : IRequestHandler<CompleteSessionCommand, SessionDetailDto>
{
    public async Task<SessionDetailDto> Handle(CompleteSessionCommand request, CancellationToken ct)
    {
        var session = await sessionRepo.GetByIdAndTrainerAsync(request.SessionId, request.TrainerId, ct)
            ?? throw new NotFoundException("Session", request.SessionId);

        session.Complete();
        await sessionRepo.UpdateAsync(session, ct);
        return session.ToDetailDto();
    }
}
