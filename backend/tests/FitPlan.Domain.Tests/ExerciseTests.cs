using FluentAssertions;
using FitPlan.Domain.ValueObjects;
using Xunit;

namespace FitPlan.Domain.Tests;

public class ExerciseTests
{
    [Fact]
    public void LogActuals_ShouldReturnNewExercise_WithActualValues()
    {
        var exercise = new Exercise("Squat", 0, 3, 8, 100.0);
        var updated = exercise.LogActuals(3, 8, 110.0, "Felt strong");

        updated.ActualWeight.Should().Be(110.0);
        updated.Notes.Should().Be("Felt strong");
        updated.UnexpectedProgress.Should().BeTrue();
    }

    [Fact]
    public void LogActuals_ShouldNotMutateOriginal()
    {
        var exercise = new Exercise("Squat", 0, 3, 8, 100.0);
        _ = exercise.LogActuals(3, 8, 110.0);

        exercise.ActualWeight.Should().BeNull();
        exercise.UnexpectedProgress.Should().BeFalse();
    }

    [Fact]
    public void LogActuals_ShouldHandleBodyweightExercise_NoProgressFlag()
    {
        var exercise = new Exercise("Pull-ups", 0, 3, 10, null);
        var updated = exercise.LogActuals(3, 12, null);

        updated.UnexpectedProgress.Should().BeTrue(); // reps exceeded
    }
}
