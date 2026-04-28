using FitPlan.Application.Common.Models;
using FitPlan.Domain.Entities;
using FitPlan.Domain.ValueObjects;

namespace FitPlan.Application.Common.Mappings;

public static class SessionMappings
{
    public static SessionDetailDto ToDetailDto(this WorkoutSession s) => new(
        s.Id, s.ClientId, s.TrainerId, s.ScheduledAt,
        s.Status.ToString(),
        s.Exercises.Select(e => e.ToDto()).ToList().AsReadOnly(),
        s.StartedAt, s.CompletedAt);

    public static ExerciseDto ToDto(this Exercise e) => new(
        e.Id, e.Name, e.Order, e.Sets, e.Reps, e.TargetWeight,
        e.ActualSets, e.ActualReps, e.ActualWeight, e.UnexpectedProgress, e.Notes);
}
