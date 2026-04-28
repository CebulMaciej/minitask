using FitPlan.Domain.Entities;

namespace FitPlan.Application.Common.Interfaces;

public interface ITrainerRepository : IRepository<Trainer>
{
    Task<Trainer?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Trainer?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default);
}
