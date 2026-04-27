using FitPlan.Domain.Common;
using FitPlan.Domain.Enums;
using FitPlan.Domain.Exceptions;
using FitPlan.Domain.ValueObjects;

namespace FitPlan.Domain.Entities;

public class WorkoutSession : BaseEntity
{
    public string TrainerId { get; private set; }
    public string ClientId { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public SessionStatus Status { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private List<Exercise> _exercises = [];
    public IReadOnlyList<Exercise> Exercises => _exercises.AsReadOnly();

    private WorkoutSession() { TrainerId = string.Empty; ClientId = string.Empty; }

    public WorkoutSession(string trainerId, string clientId, DateTime scheduledAt, IEnumerable<Exercise> exercises)
    {
        TrainerId = trainerId;
        ClientId = clientId;
        ScheduledAt = scheduledAt;
        Status = SessionStatus.Planned;
        _exercises.AddRange(exercises);
    }

    public void Update(DateTime scheduledAt, IEnumerable<Exercise> exercises)
    {
        if (Status != SessionStatus.Planned)
            throw new DomainException("Only planned sessions can be updated.");
        ScheduledAt = scheduledAt;
        _exercises.Clear();
        _exercises.AddRange(exercises);
        Touch();
    }

    public void Start()
    {
        if (Status != SessionStatus.Planned)
            throw new DomainException("Only planned sessions can be started.");
        Status = SessionStatus.InProgress;
        StartedAt = DateTime.UtcNow;
        Touch();
    }

    public void LogExerciseActuals(string exerciseId, int? actualSets, int? actualReps, double? actualWeight, string? notes)
    {
        if (Status != SessionStatus.InProgress)
            throw new DomainException("Can only log actuals during an in-progress session.");

        var index = _exercises.FindIndex(e => e.Id == exerciseId);
        if (index < 0) throw new DomainException($"Exercise {exerciseId} not found in session.");

        _exercises[index] = _exercises[index].LogActuals(actualSets, actualReps, actualWeight, notes);
        Touch();
    }

    public void Complete()
    {
        if (Status != SessionStatus.InProgress)
            throw new DomainException("Only in-progress sessions can be completed.");
        Status = SessionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Touch();
    }

    public void Cancel()
    {
        if (Status == SessionStatus.Completed)
            throw new DomainException("Completed sessions cannot be cancelled.");
        Status = SessionStatus.Cancelled;
        Touch();
    }

    public void SetExercises(IEnumerable<Exercise> exercises)
    {
        _exercises.Clear();
        _exercises.AddRange(exercises);
    }
}
