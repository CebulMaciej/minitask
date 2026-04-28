using FluentAssertions;
using Moq;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Application.Sessions.Commands;
using FitPlan.Domain.Entities;
using FitPlan.Domain.ValueObjects;
using Xunit;

namespace FitPlan.Application.Tests.Sessions;

public class LiveSessionHandlerTests
{
    private readonly Mock<ISessionRepository> _sessionRepo = new();
    private const string TrainerId = "trainer-1";
    private const string ClientId = "client-1";

    private static WorkoutSession PlannedSession()
    {
        var exercises = new[]
        {
            new Exercise("Bench Press", 0, 3, 8, 100.0),
            new Exercise("Squat", 1, 4, 5, 120.0)
        };
        return new WorkoutSession(TrainerId, ClientId, DateTime.UtcNow.AddHours(1), exercises);
    }

    // ── StartSession ────────────────────────────────────────────────

    [Fact]
    public async Task StartSession_ShouldReturnInProgress_WhenSessionIsPlanned()
    {
        var session = PlannedSession();
        _sessionRepo.Setup(r => r.GetByIdAndTrainerAsync(session.Id, TrainerId, default))
            .ReturnsAsync(session);

        var result = await new StartSessionHandler(_sessionRepo.Object).Handle(
            new StartSessionCommand(TrainerId, session.Id), default);

        result.Status.Should().Be("InProgress");
        _sessionRepo.Verify(r => r.UpdateAsync(session, default), Times.Once);
    }

    [Fact]
    public async Task StartSession_ShouldThrowNotFound_WhenSessionDoesNotExist()
    {
        _sessionRepo.Setup(r => r.GetByIdAndTrainerAsync(It.IsAny<string>(), TrainerId, default))
            .ReturnsAsync((WorkoutSession?)null);

        var act = () => new StartSessionHandler(_sessionRepo.Object).Handle(
            new StartSessionCommand(TrainerId, "ghost-id"), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── LogExerciseActuals ──────────────────────────────────────────

    [Fact]
    public async Task LogActuals_ShouldFlagUnexpectedProgress_WhenActualExceedsPlanned()
    {
        var session = PlannedSession();
        session.Start();
        var exerciseId = session.Exercises[0].Id; // Bench Press: planned 100kg

        _sessionRepo.Setup(r => r.GetByIdAndTrainerAsync(session.Id, TrainerId, default))
            .ReturnsAsync(session);

        var result = await new LogExerciseActualsHandler(_sessionRepo.Object).Handle(
            new LogExerciseActualsCommand(TrainerId, session.Id, exerciseId,
                ActualSets: 3, ActualReps: 8, ActualWeight: 120.0, Notes: null),
            default);

        result.UnexpectedProgress.Should().BeTrue();
        result.ActualWeight.Should().Be(120.0);
    }

    [Fact]
    public async Task LogActuals_ShouldNotFlagProgress_WhenActualsMeetPlan()
    {
        var session = PlannedSession();
        session.Start();
        var exerciseId = session.Exercises[0].Id;

        _sessionRepo.Setup(r => r.GetByIdAndTrainerAsync(session.Id, TrainerId, default))
            .ReturnsAsync(session);

        var result = await new LogExerciseActualsHandler(_sessionRepo.Object).Handle(
            new LogExerciseActualsCommand(TrainerId, session.Id, exerciseId,
                ActualSets: 3, ActualReps: 8, ActualWeight: 100.0, Notes: null),
            default);

        result.UnexpectedProgress.Should().BeFalse();
    }

    [Fact]
    public async Task LogActuals_ShouldThrowNotFound_WhenExerciseIdInvalid()
    {
        var session = PlannedSession();
        session.Start();

        _sessionRepo.Setup(r => r.GetByIdAndTrainerAsync(session.Id, TrainerId, default))
            .ReturnsAsync(session);

        var act = () => new LogExerciseActualsHandler(_sessionRepo.Object).Handle(
            new LogExerciseActualsCommand(TrainerId, session.Id, "invalid-exercise-id",
                3, 8, 100.0, null),
            default);

        await act.Should().ThrowAsync<Exception>(); // DomainException from session.LogExerciseActuals
    }

    // ── CompleteSession ─────────────────────────────────────────────

    [Fact]
    public async Task CompleteSession_ShouldReturnCompleted_WhenSessionIsInProgress()
    {
        var session = PlannedSession();
        session.Start();

        _sessionRepo.Setup(r => r.GetByIdAndTrainerAsync(session.Id, TrainerId, default))
            .ReturnsAsync(session);

        var result = await new CompleteSessionHandler(_sessionRepo.Object).Handle(
            new CompleteSessionCommand(TrainerId, session.Id), default);

        result.Status.Should().Be("Completed");
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CompleteSession_ShouldThrowDomainException_WhenSessionIsStillPlanned()
    {
        var session = PlannedSession(); // status = PLANNED, not started

        _sessionRepo.Setup(r => r.GetByIdAndTrainerAsync(session.Id, TrainerId, default))
            .ReturnsAsync(session);

        var act = () => new CompleteSessionHandler(_sessionRepo.Object).Handle(
            new CompleteSessionCommand(TrainerId, session.Id), default);

        await act.Should().ThrowAsync<Exception>().WithMessage("*in-progress*");
    }
}
