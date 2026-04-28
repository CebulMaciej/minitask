using FitPlan.Domain.Entities;

namespace FitPlan.Application.Common.Interfaces;

public interface IClientRepository : IRepository<Client>
{
    Task<Client?> GetByEmailAndTrainerAsync(string email, string trainerId, CancellationToken ct = default);
    Task<IReadOnlyList<Client>> GetByTrainerAsync(string trainerId, CancellationToken ct = default);
    Task<Client?> GetByIdAndTrainerAsync(string id, string trainerId, CancellationToken ct = default);
}
