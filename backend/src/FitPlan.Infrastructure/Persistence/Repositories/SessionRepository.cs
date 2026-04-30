using MongoDB.Driver;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Domain.Entities;
using FitPlan.Domain.Enums;

namespace FitPlan.Infrastructure.Persistence.Repositories;

public class SessionRepository(IMongoContext context)
    : MongoRepository<WorkoutSession>(context, "workout_sessions"), ISessionRepository
{
    public async Task<IReadOnlyList<WorkoutSession>> GetByClientAsync(
        string clientId, string trainerId,
        DateTime? from, DateTime? to, SessionStatus? status,
        CancellationToken ct = default)
    {
        var filter = Builders<WorkoutSession>.Filter.Eq(s => s.ClientId, clientId)
            & Builders<WorkoutSession>.Filter.Eq(s => s.TrainerId, trainerId);

        if (from.HasValue)
            filter &= Builders<WorkoutSession>.Filter.Gte(s => s.ScheduledAt, from.Value);
        if (to.HasValue)
            filter &= Builders<WorkoutSession>.Filter.Lte(s => s.ScheduledAt, to.Value);
        if (status.HasValue)
            filter &= Builders<WorkoutSession>.Filter.Eq(s => s.Status, status.Value);

        return await Collection.Find(filter)
            .SortBy(s => s.ScheduledAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<WorkoutSession>> GetByTrainerAsync(
        string trainerId, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var filter = Builders<WorkoutSession>.Filter.Eq(s => s.TrainerId, trainerId);
        if (from.HasValue)
            filter &= Builders<WorkoutSession>.Filter.Gte(s => s.ScheduledAt, from.Value);
        if (to.HasValue)
            filter &= Builders<WorkoutSession>.Filter.Lte(s => s.ScheduledAt, to.Value);
        return await Collection.Find(filter).SortBy(s => s.ScheduledAt).ToListAsync(ct);
    }

    public async Task<WorkoutSession?> GetByIdAndTrainerAsync(string id, string trainerId, CancellationToken ct = default)
        => await FindOneAsync(s => s.Id == id && s.TrainerId == trainerId, ct);

    public async Task<WorkoutSession?> GetByIdAndClientAsync(string id, string clientId, CancellationToken ct = default)
        => await FindOneAsync(s => s.Id == id && s.ClientId == clientId, ct);

    public async Task<IReadOnlyList<WorkoutSession>> GetCompletedByClientAsync(
        string clientId, int page, int limit, CancellationToken ct = default)
        => await Collection.Find(s => s.ClientId == clientId && s.Status == SessionStatus.Completed)
            .SortByDescending(s => s.CompletedAt)
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToListAsync(ct);

    public async Task<int> CountCompletedByClientAsync(string clientId, CancellationToken ct = default)
        => (int)await Collection.CountDocumentsAsync(
            s => s.ClientId == clientId && s.Status == SessionStatus.Completed, cancellationToken: ct);

    public async Task<DateTime?> GetLastCompletedAtByClientAsync(string clientId, CancellationToken ct = default)
    {
        var session = await Collection
            .Find(s => s.ClientId == clientId && s.Status == SessionStatus.Completed)
            .SortByDescending(s => s.CompletedAt)
            .Limit(1)
            .FirstOrDefaultAsync(ct);
        return session?.CompletedAt;
    }
}
