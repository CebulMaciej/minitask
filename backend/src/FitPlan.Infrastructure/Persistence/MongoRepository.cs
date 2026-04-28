using System.Linq.Expressions;
using MongoDB.Driver;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Domain.Common;

namespace FitPlan.Infrastructure.Persistence;

public abstract class MongoRepository<T>(IMongoContext context, string collectionName)
    : IRepository<T> where T : BaseEntity
{
    protected readonly IMongoCollection<T> Collection = context.GetCollection<T>(collectionName);

    public async Task<T?> GetByIdAsync(string id, CancellationToken ct = default)
        => await Collection.Find(e => e.Id == id).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await Collection.Find(_ => true).ToListAsync(ct);

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await Collection.Find(predicate).ToListAsync(ct);

    public async Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await Collection.Find(predicate).FirstOrDefaultAsync(ct);

    public async Task InsertAsync(T entity, CancellationToken ct = default)
        => await Collection.InsertOneAsync(entity, cancellationToken: ct);

    public async Task UpdateAsync(T entity, CancellationToken ct = default)
        => await Collection.ReplaceOneAsync(e => e.Id == entity.Id, entity, cancellationToken: ct);

    public async Task DeleteAsync(string id, CancellationToken ct = default)
        => await Collection.DeleteOneAsync(e => e.Id == id, ct);

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await Collection.Find(predicate).AnyAsync(ct);
}
