using FitPlan.Domain.Entities;
using FitPlan.Domain.Enums;

namespace FitPlan.Application.Common.Interfaces;

public interface ISessionRepository : IRepository<WorkoutSession>
{
    Task<IReadOnlyList<WorkoutSession>> GetByClientAsync(string clientId, string trainerId,
        DateTime? from = null, DateTime? to = null, SessionStatus? status = null,
        CancellationToken ct = default);

    Task<WorkoutSession?> GetByIdAndTrainerAsync(string id, string trainerId, CancellationToken ct = default);
    Task<WorkoutSession?> GetByIdAndClientAsync(string id, string clientId, CancellationToken ct = default);

    Task<IReadOnlyList<WorkoutSession>> GetCompletedByClientAsync(string clientId,
        int page, int limit, CancellationToken ct = default);

    Task<int> CountCompletedByClientAsync(string clientId, CancellationToken ct = default);
}
