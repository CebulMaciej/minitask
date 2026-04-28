using FitPlan.Domain.Enums;

namespace FitPlan.Application.Common.Models;

public record AuthResult(string AccessToken, string RefreshToken, UserType UserType);

public record ExerciseDto(
    string Id, string Name, int Order, int Sets, int Reps, double? TargetWeight,
    int? ActualSets, int? ActualReps, double? ActualWeight, bool UnexpectedProgress, string? Notes);

public record SessionSummaryDto(string Id, DateTime ScheduledAt, string Status, int ExerciseCount, DateTime? CompletedAt);

public record SessionDetailDto(
    string Id, string ClientId, string TrainerId, DateTime ScheduledAt,
    string Status, IReadOnlyList<ExerciseDto> Exercises, DateTime? StartedAt, DateTime? CompletedAt);

public record ClientSummaryDto(string Id, string Name, string Email, DateTime CreatedAt);

public record ClientDetailDto(string Id, string Name, string Email, DateTime CreatedAt, int TotalSessions, DateTime? LastSessionAt);

public record PaginatedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int Limit);
