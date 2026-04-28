using FluentValidation;
using MediatR;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Application.Common.Mappings;
using FitPlan.Application.Common.Models;
using FitPlan.Domain.Entities;
using FitPlan.Domain.ValueObjects;

namespace FitPlan.Application.Sessions.Commands;

public record ExerciseInput(string Name, int Sets, int Reps, double? TargetWeight);

public record CreateSessionCommand(
    string TrainerId,
    string ClientId,
    DateTime ScheduledAt,
    IReadOnlyList<ExerciseInput> Exercises) : IRequest<SessionDetailDto>, ITenantScopedRequest;

public class CreateSessionValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionValidator()
    {
        RuleFor(x => x.ScheduledAt).NotEmpty();
        RuleFor(x => x.Exercises).NotEmpty().WithMessage("At least one exercise is required.");
        RuleForEach(x => x.Exercises).ChildRules(e =>
        {
            e.RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            e.RuleFor(x => x.Sets).GreaterThan(0);
            e.RuleFor(x => x.Reps).GreaterThan(0);
        });
    }
}

public class CreateSessionHandler(
    IClientRepository clientRepo,
    ISessionRepository sessionRepo) : IRequestHandler<CreateSessionCommand, SessionDetailDto>
{
    public async Task<SessionDetailDto> Handle(CreateSessionCommand request, CancellationToken ct)
    {
        var client = await clientRepo.GetByIdAndTrainerAsync(request.ClientId, request.TrainerId, ct)
            ?? throw new NotFoundException("Client", request.ClientId);

        var exercises = request.Exercises
            .Select((e, i) => new Exercise(e.Name, i, e.Sets, e.Reps, e.TargetWeight))
            .ToList();

        var session = new WorkoutSession(request.TrainerId, request.ClientId, request.ScheduledAt, exercises);
        await sessionRepo.InsertAsync(session, ct);

        return session.ToDetailDto();
    }
}
