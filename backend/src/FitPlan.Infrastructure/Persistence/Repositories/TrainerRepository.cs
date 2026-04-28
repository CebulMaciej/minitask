using FitPlan.Application.Common.Interfaces;
using FitPlan.Domain.Entities;

namespace FitPlan.Infrastructure.Persistence.Repositories;

public class TrainerRepository(IMongoContext context)
    : MongoRepository<Trainer>(context, "trainers"), ITrainerRepository
{
    public async Task<Trainer?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await FindOneAsync(t => t.Email == email.ToLowerInvariant(), ct);

    public async Task<Trainer?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default)
        => await FindOneAsync(t => t.GoogleId == googleId, ct);
}
