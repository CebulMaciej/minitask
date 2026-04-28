using FluentAssertions;
using FitPlan.Domain.Entities;
using FitPlan.Domain.Enums;
using FitPlan.Domain.Exceptions;
using FitPlan.Domain.ValueObjects;
using Xunit;

namespace FitPlan.Domain.Tests;

public class WorkoutSessionTests
{
    private static WorkoutSession CreatePlannedSession()
    {
        var exercises = new[] { new Exercise("Bench Press", 0, 3, 10, 60.0) };
        return new WorkoutSession("trainer-1", "client-1", DateTime.UtcNow.AddDays(1), exercises);
    }

    // ── Status transitions ──────────────────────────────────────────

    [Fact]
    public void Start_ShouldTransitionToInProgress_WhenPlanned()
    {
        var session = CreatePlannedSession();
        session.Start();
        session.Status.Should().Be(SessionStatus.InProgress);
        session.StartedAt.Should().NotBeNull();
    }

    [Fact]
    public void Start_ShouldThrow_WhenAlreadyInProgress()
    {
        var session = CreatePlannedSession();
        session.Start();
        var act = () => session.Start();
        act.Should().Throw<DomainException>().WithMessage("*planned*");
    }

    [Fact]
    public void Complete_ShouldTransitionToCompleted_WhenInProgress()
    {
        var session = CreatePlannedSession();
        session.Start();
        session.Complete();
        session.Status.Should().Be(SessionStatus.Completed);
        session.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_ShouldThrow_WhenPlanned()
    {
        var session = CreatePlannedSession();
        var act = () => session.Complete();
        act.Should().Throw<DomainException>().WithMessage("*in-progress*");
    }

    [Fact]
    public void Update_ShouldThrow_WhenInProgress()
    {
        var session = CreatePlannedSession();
        session.Start();
        var act = () => session.Update(DateTime.UtcNow, []);
        act.Should().Throw<DomainException>().WithMessage("*planned*");
    }

    // ── Unexpected progress ─────────────────────────────────────────

    [Fact]
    public void LogActuals_ShouldFlagUnexpectedProgress_WhenWeightExceedsTarget()
    {
        var session = CreatePlannedSession();
        session.Start();
        var exerciseId = session.Exercises[0].Id;

        session.LogExerciseActuals(exerciseId, 3, 10, 70.0, null);

        session.Exercises[0].UnexpectedProgress.Should().BeTrue();
        session.Exercises[0].ActualWeight.Should().Be(70.0);
    }

    [Fact]
    public void LogActuals_ShouldFlagUnexpectedProgress_WhenRepsExceedTarget()
    {
        var session = CreatePlannedSession();
        session.Start();
        var exerciseId = session.Exercises[0].Id;

        session.LogExerciseActuals(exerciseId, 3, 15, 60.0, null);

        session.Exercises[0].UnexpectedProgress.Should().BeTrue();
    }

    [Fact]
    public void LogActuals_ShouldNotFlagProgress_WhenActualsMatchPlan()
    {
        var session = CreatePlannedSession();
        session.Start();
        var exerciseId = session.Exercises[0].Id;

        session.LogExerciseActuals(exerciseId, 3, 10, 60.0, null);

        session.Exercises[0].UnexpectedProgress.Should().BeFalse();
    }

    [Fact]
    public void LogActuals_ShouldThrow_WhenSessionNotInProgress()
    {
        var session = CreatePlannedSession();
        var exerciseId = session.Exercises[0].Id;
        var act = () => session.LogExerciseActuals(exerciseId, 3, 10, 60.0, null);
        act.Should().Throw<DomainException>().WithMessage("*in-progress*");
    }

    [Fact]
    public void LogActuals_ShouldThrow_WhenExerciseNotFound()
    {
        var session = CreatePlannedSession();
        session.Start();
        var act = () => session.LogExerciseActuals("nonexistent", 3, 10, 60.0, null);
        act.Should().Throw<DomainException>();
    }
}
