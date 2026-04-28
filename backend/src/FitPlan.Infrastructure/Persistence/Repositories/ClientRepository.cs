using FitPlan.Application.Common.Interfaces;
using FitPlan.Domain.Entities;

namespace FitPlan.Infrastructure.Persistence.Repositories;

public class ClientRepository(IMongoContext context)
    : MongoRepository<Client>(context, "clients"), IClientRepository
{
    public async Task<Client?> GetByEmailAndTrainerAsync(string email, string trainerId, CancellationToken ct = default)
        => await FindOneAsync(c => c.Email == email.ToLowerInvariant() && c.TrainerId == trainerId, ct);

    public async Task<IReadOnlyList<Client>> GetByTrainerAsync(string trainerId, CancellationToken ct = default)
        => await FindAsync(c => c.TrainerId == trainerId, ct);

    public async Task<Client?> GetByIdAndTrainerAsync(string id, string trainerId, CancellationToken ct = default)
        => await FindOneAsync(c => c.Id == id && c.TrainerId == trainerId, ct);
}
